using System.Collections.Concurrent;
using System.Diagnostics;
using BitFaster.Caching.Lru;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.BitFaster;

/// <summary>
/// <see cref="ICacheService"/> backed by BitFaster's <see cref="ConcurrentLru{TKey,TValue}"/>.
/// The BitFaster dependency is isolated to this package.
/// </summary>
public sealed class BitFasterCacheService : ICacheService
{
    private const string ProviderName = "BitFaster";
    private const string CacheType = "Memory";
    
    private readonly ConcurrentLru<string, object?> _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();
    private readonly ConcurrentDictionary<string, InMemoryCacheEntryMetadata> _metadata = new();

    /// <summary>Creates the cache with capacity from <see cref="BitFasterCacheOptions"/>.</summary>
    public BitFasterCacheService(IOptions<BitFasterCacheOptions> options)
    {
        var capacity = Math.Max(1, options.Value.Capacity);
        _cache = new ConcurrentLru<string, object?>(capacity);
    }

    /// <inheritdoc />
    public bool TryGet<T>(string key, out T value, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (_cache.TryGet(key, out var stored) && stored is T typed)
            {
                UpdateMetadataForHit(key);
                value = typed;
                return true;
            }

            UpdateMetadataForMiss(key);
            value = default!;
            return false;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <inheritdoc />
    public T? Get<T>(string key, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (TryGet<T>(key, out var value, ct))
            {
                return value;
            }
            return default;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        return await Task.Run(() => Get<T>(key, ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public CacheResult<T> GetWithResult<T>(string key, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (TryGet<T>(key, out var value, ct))
            {
                var metadata = GetMetadata(key, ct);
                return CacheResult<T>.Success(value, metadata, ProviderName, CacheType, stopwatch.Elapsed);
            }
            
            return CacheResult<T>.NotFound(key, ProviderName, CacheType, stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <inheritdoc />
    public async Task<CacheResult<T>> GetWithResultAsync<T>(string key, CancellationToken ct = default)
    {
        return await Task.Run(() => GetWithResult<T>(key, ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        // ConcurrentLru evicts by recency/capacity; per-entry TTL is not modeled by this backend.
        _cache.AddOrUpdate(key, value);
        
        // Create or update metadata
        var now = DateTime.UtcNow;
        var metadata = new InMemoryCacheEntryMetadata(
            key: key,
            createdAtUtc: now,
            lastAccessedUtc: now,
            expiresAtUtc: ttl.HasValue ? now.Add(ttl.Value) : null,
            sizeBytes: null, // Not tracked for in-memory cache
            isFresh: true,
            accessCount: 1,
            hitCount: 0,
            missCount: 0);
        
        _metadata.AddOrUpdate(key, metadata, (_, _) => metadata);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        await Task.Run(() => Set(key, value, ttl, ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Remove(string key, CancellationToken ct = default)
    {
        _cache.TryRemove(key);
        _metadata.TryRemove(key, out _);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await Task.Run(() => Remove(key, ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Clear(CancellationToken ct = default)
    {
        _cache.Clear();
        _metadata.Clear();
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct = default)
    {
        await Task.Run(() => Clear(ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetKeys(CancellationToken ct = default)
    {
        return _cache.Keys.ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetKeysAsync(CancellationToken ct = default)
    {
        return await Task.Run(() => GetKeys(ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public CacheEntryMetadata? GetMetadata(string key, CancellationToken ct = default)
    {
        return _metadata.TryGetValue(key, out var metadata) ? metadata : null;
    }

    /// <inheritdoc />
    public async Task<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken ct = default)
    {
        return await Task.Run(() => GetMetadata(key, ct), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        if (TryGet<T>(key, out var existing, ct))
        {
            return existing;
        }

        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        keyLock.Wait(ct);

        try
        {
            if (TryGet<T>(key, out existing, ct))
            {
                return existing;
            }

            var created = factory();
            Set(key, created, ttl, ct);
            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        if (TryGet<T>(key, out var existing, ct))
        {
            return existing;
        }

        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(ct).ConfigureAwait(false);
        
        try
        {
            if (TryGet<T>(key, out existing, ct))
            {
                return existing;
            }

            var created = await factory().ConfigureAwait(false);
            Set(key, created, ttl, ct);
            return created;
        }
        finally
        {
            keyLock.Release();
        }
    }

    private void UpdateMetadataForHit(string key)
    {
        if (_metadata.TryGetValue(key, out var metadata))
        {
            metadata.UpdateAccess(isHit: true);
        }
        else
        {
            // Create metadata for existing entry without creation time
            var now = DateTime.UtcNow;
            metadata = new InMemoryCacheEntryMetadata(
                key: key,
                createdAtUtc: now,
                lastAccessedUtc: now,
                expiresAtUtc: null,
                sizeBytes: null,
                isFresh: true,
                accessCount: 1,
                hitCount: 1,
                missCount: 0);
            _metadata.TryAdd(key, metadata);
        }
    }

    private void UpdateMetadataForMiss(string key)
    {
        if (_metadata.TryGetValue(key, out var metadata))
        {
            metadata.UpdateAccess(isHit: false);
        }
        else
        {
            // Create metadata for miss
            var now = DateTime.UtcNow;
            metadata = new InMemoryCacheEntryMetadata(
                key: key,
                createdAtUtc: now,
                lastAccessedUtc: now,
                expiresAtUtc: null,
                sizeBytes: null,
                isFresh: false,
                accessCount: 1,
                hitCount: 0,
                missCount: 1);
            _metadata.TryAdd(key, metadata);
        }
    }
}

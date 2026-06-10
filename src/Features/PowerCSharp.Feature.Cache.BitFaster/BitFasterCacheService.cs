using BitFaster.Caching.Lru;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache;

namespace PowerCSharp.Feature.Cache.BitFaster;

/// <summary>
/// <see cref="ICacheService"/> backed by BitFaster's <see cref="ConcurrentLru{TKey,TValue}"/>.
/// The BitFaster dependency is isolated to this package.
/// </summary>
public sealed class BitFasterCacheService : ICacheService
{
    private readonly ConcurrentLru<string, object?> _cache;

    /// <summary>Creates the cache with capacity from <see cref="CacheFeatureOptions"/>.</summary>
    public BitFasterCacheService(IOptions<CacheFeatureOptions> options)
    {
        var capacity = Math.Max(1, options.Value.Capacity);
        _cache = new ConcurrentLru<string, object?>(capacity);
    }

    /// <inheritdoc />
    public bool TryGet<T>(string key, out T value)
    {
        if (_cache.TryGet(key, out var stored) && stored is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        // ConcurrentLru evicts by recency/capacity; per-entry TTL is not modeled by this backend.
        _cache.AddOrUpdate(key, value);
    }

    /// <inheritdoc />
    public void Remove(string key) => _cache.TryRemove(key);

    /// <inheritdoc />
    public void Clear() => _cache.Clear();

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetKeys() => _cache.Keys.ToArray();
}

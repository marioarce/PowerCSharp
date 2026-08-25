using Microsoft.Extensions.Logging;

namespace PowerCSharp.Feature.Cache.Abstractions.NoOp;

/// <summary>
/// Inert <see cref="IDiskCacheService"/> used when the Cache feature is disabled or no provider is
/// configured. Mirrors the proven NoOp disk-cache fallback pattern.
/// </summary>
public sealed class NoOpDiskCacheService : IDiskCacheService
{
    private const string ProviderName = "NoOp";
    private const string CacheType = "Disk";

    /// <summary>Creates the NoOp disk cache and logs that disk caching is inert.</summary>
    public NoOpDiskCacheService(ILogger<NoOpDiskCacheService> logger)
    {
        // NOTE: see the matching comment in NoOpCacheService — this can be constructed by
        // ASP.NET Core's ValidateOnBuild even when a real provider (e.g. Disk) is the one
        // actually injected as IDiskCacheService. Check the Cache feature's "Cache feature
        // resolved" diagnostic line for the implementation actually in use.
        logger.LogInformation("NoOp disk cache constructed (IDiskCacheService safe-off floor).");
    }

    /// <inheritdoc />
    public ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default)
        => new(default(T));

    /// <inheritdoc />
    public ValueTask<T?> GetAsync<T>(string key, CacheFileKind fileKind, CancellationToken ct = default)
        => new(default(T));

    /// <inheritdoc />
    public T? Get<T>(string key, CancellationToken ct = default)
    {
        return default;
    }

    /// <inheritdoc />
    public T? Get<T>(string key, CacheFileKind fileKind, CancellationToken ct = default)
    {
        return default;
    }

    /// <inheritdoc />
    public ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default)
        => default;

    /// <inheritdoc />
    public ValueTask SetAsync<T>(string key, T value, CacheFileKind fileKind, CancellationToken ct = default)
        => default;

    /// <inheritdoc />
    public void Set<T>(string key, T value, CancellationToken ct = default)
    {
        // NoOp - discard the value
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, CacheFileKind fileKind, CancellationToken ct = default)
    {
        // NoOp - discard the value
    }

    /// <inheritdoc />
    public ValueTask<CacheResult<T>> GetWithResultAsync<T>(string key, CancellationToken ct = default)
        => new(CacheResult<T>.NotFound(key, ProviderName, CacheType));

    /// <inheritdoc />
    public ValueTask<CacheResult<T>> GetWithResultAsync<T>(string key, CacheFileKind fileKind, CancellationToken ct = default)
        => new(CacheResult<T>.NotFound(key, ProviderName, CacheType));

    /// <inheritdoc />
    public CacheResult<T> GetWithResult<T>(string key, CancellationToken ct = default)
    {
        return CacheResult<T>.NotFound(key, ProviderName, CacheType);
    }

    /// <inheritdoc />
    public void Remove(string key, CancellationToken ct = default)
    {
        // NoOp - nothing to remove
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Clear(CancellationToken ct = default)
    {
        // NoOp - nothing to clear
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetKeys(CancellationToken ct = default)
    {
        return Array.Empty<string>();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetKeysAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return Array.Empty<string>();
    }

    /// <inheritdoc />
    public CacheEntryMetadata? GetMetadata(string key, CancellationToken ct = default)
    {
        return null;
    }

    /// <inheritdoc />
    public async Task<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return null;
    }

    /// <inheritdoc />
    public ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CacheFileKind fileKind, CancellationToken ct = default)
        => new((CacheEntryMetadata?)null);

    /// <inheritdoc />
    public ValueTask<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        return new ValueTask<T>(factory());
    }

    /// <inheritdoc />
    public ValueTask<T> GetOrCreateAsync<T>(string key, CacheFileKind fileKind, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        return new ValueTask<T>(factory());
    }

    /// <inheritdoc />
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        return factory();
    }
}

using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.NoOp;

/// <summary>
/// Inert <see cref="IDiskCacheService"/> used when the Cache feature is disabled or no provider is
/// configured. Mirrors the proven NoOp disk-cache fallback pattern.
/// </summary>
public sealed class NoOpDiskCacheService : IDiskCacheService
{
    /// <summary>Creates the NoOp disk cache and logs that disk caching is inert.</summary>
    public NoOpDiskCacheService(ILogger<NoOpDiskCacheService> logger)
    {
        logger.LogInformation("Cache feature is disabled or unconfigured; using NoOp disk cache.");
    }

    /// <inheritdoc />
    public ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => new(default(T));

    /// <inheritdoc />
    public ValueTask SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => default;

    /// <inheritdoc />
    public ValueTask<T?> GetAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
        => new(default(T));

    /// <inheritdoc />
    public ValueTask SetAsync<T>(string key, T value, CacheFileKind fileKind, CancellationToken cancellationToken = default)
        => default;

    /// <inheritdoc />
    public ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CancellationToken cancellationToken = default)
        => new(CacheResult<T>.NotFound(key));

    /// <inheritdoc />
    public ValueTask<CacheResult<T>> GetWithMetadataAsync<T>(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
        => new(CacheResult<T>.NotFound(key));

    /// <inheritdoc />
    public ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
        => new((CacheEntryMetadata?)null);

    /// <inheritdoc />
    public ValueTask<CacheEntryMetadata?> GetMetadataAsync(string key, CacheFileKind fileKind, CancellationToken cancellationToken = default)
        => new((CacheEntryMetadata?)null);
}

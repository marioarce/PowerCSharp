using Microsoft.Extensions.Logging;

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
}

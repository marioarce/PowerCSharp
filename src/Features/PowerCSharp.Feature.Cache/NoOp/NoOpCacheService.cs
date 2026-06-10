using Microsoft.Extensions.Logging;

namespace PowerCSharp.Feature.Cache.NoOp;

/// <summary>
/// Inert <see cref="ICacheService"/> used when the Cache feature is disabled or no provider is
/// configured. Every read is a miss and writes are discarded, so dependents resolve safely.
/// </summary>
public sealed class NoOpCacheService : ICacheService
{
    /// <summary>Creates the NoOp cache and logs that caching is inert.</summary>
    public NoOpCacheService(ILogger<NoOpCacheService> logger)
    {
        logger.LogInformation("Cache feature is disabled or unconfigured; using NoOp in-memory cache.");
    }

    /// <inheritdoc />
    public bool TryGet<T>(string key, out T value)
    {
        value = default!;
        return false;
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
    }

    /// <inheritdoc />
    public void Clear()
    {
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetKeys() => Array.Empty<string>();

    /// <inheritdoc />
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null) => factory();

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
        => await factory().ConfigureAwait(false);
}

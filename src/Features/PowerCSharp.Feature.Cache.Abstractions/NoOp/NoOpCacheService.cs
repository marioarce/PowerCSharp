using Microsoft.Extensions.Logging;

namespace PowerCSharp.Feature.Cache.Abstractions.NoOp;

/// <summary>
/// Inert <see cref="ICacheService"/> used when the Cache feature is disabled or no provider is
/// configured. Every read is a miss and writes are discarded, so dependents resolve safely.
/// </summary>
public sealed class NoOpCacheService : ICacheService
{
    private const string ProviderName = "NoOp";
    private const string CacheType = "Memory";

    /// <summary>Creates the NoOp cache and logs that caching is inert.</summary>
    public NoOpCacheService(ILogger<NoOpCacheService> logger)
    {
        // NOTE: this constructor can run even when a real provider (e.g. BitFaster) is the one
        // actually injected as ICacheService — ASP.NET Core's ValidateOnBuild (on by default in
        // Development) constructs every registered descriptor, including this shadowed safe-off
        // floor, purely to validate the DI graph. Seeing this log does not by itself mean the app
        // is using NoOp; check the Cache feature's "Cache feature resolved" diagnostic line for
        // the implementation actually bound to ICacheService.
        logger.LogInformation("NoOp in-memory cache constructed (ICacheService safe-off floor).");
    }

    /// <inheritdoc />
    public bool TryGet<T>(string key, out T value, CancellationToken ct = default)
    {
        value = default!;
        return false;
    }

    /// <inheritdoc />
    public T? Get<T>(string key, CancellationToken ct = default)
    {
        return default;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return default;
    }

    /// <inheritdoc />
    public CacheResult<T> GetWithResult<T>(string key, CancellationToken ct = default)
    {
        return CacheResult<T>.NotFound(key, ProviderName, CacheType);
    }

    /// <inheritdoc />
    public async Task<CacheResult<T>> GetWithResultAsync<T>(string key, CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return CacheResult<T>.NotFound(key, ProviderName, CacheType);
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        // NoOp - discard the value
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
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
    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        return factory();
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        return await factory().ConfigureAwait(false);
    }
}

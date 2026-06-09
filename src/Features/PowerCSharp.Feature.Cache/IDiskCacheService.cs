namespace PowerCSharp.Feature.Cache;

/// <summary>
/// Asynchronous disk-backed cache abstraction. Concrete backends are supplied by provider packages;
/// a NoOp is used when disabled or unconfigured.
/// </summary>
public interface IDiskCacheService
{
    /// <summary>Gets a value by key, or <c>default</c> when absent.</summary>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Sets a value by key.</summary>
    ValueTask SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);
}

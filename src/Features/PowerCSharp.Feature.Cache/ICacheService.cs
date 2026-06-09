namespace PowerCSharp.Feature.Cache;

/// <summary>
/// In-memory cache abstraction. Concrete backends are supplied by provider packages
/// (e.g. <c>PowerCSharp.Feature.Cache.BitFaster</c>); a NoOp is used when disabled or unconfigured.
/// </summary>
public interface ICacheService
{
    /// <summary>Attempts to get a cached value by key.</summary>
    bool TryGet<T>(string key, out T value);

    /// <summary>Sets a value, with an optional time-to-live.</summary>
    void Set<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>Removes a cached value by key.</summary>
    void Remove(string key);
}

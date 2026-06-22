namespace PowerCSharp.Feature.Cache.Abstractions.Enums;

/// <summary>
/// Represents the priority of a cache entry for eviction decisions.
/// </summary>
public enum CacheEntryPriority
{
    /// <summary>
    /// Low priority - likely to be evicted first.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority - standard eviction behavior.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority - less likely to be evicted.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - very unlikely to be evicted.
    /// </summary>
    Critical = 3
}

namespace PowerCSharp.Feature.Cache.Abstractions.Enums;

/// <summary>
/// Represents the reason for a cache operation result.
/// </summary>
public enum CacheResultReason
{
    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The cache entry was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The cache entry was expired.
    /// </summary>
    Expired,

    /// <summary>
    /// An error occurred during the operation.
    /// </summary>
    Error
}

namespace PowerCSharp.Feature.Cache.Abstractions.Enums;

/// <summary>
/// Selects the cache backend. Drives provider selection via the <c>PowerFeatures:Cache:Provider</c>
/// variant flag. Provider implementations live in their own packages.
/// </summary>
public enum CacheProvider
{
    /// <summary>No backend; NoOp implementations are used.</summary>
    None = 0,

    /// <summary>BitFaster-backed implementation (<c>PowerCSharp.Feature.Cache.BitFaster</c>).</summary>
    BitFaster = 1,

    /// <summary>Native <c>MemoryCache</c>-backed implementation (future).</summary>
    Memory = 2,
}

namespace PowerCSharp.Feature.Cache.BitFaster;

/// <summary>
/// Options for the BitFaster in-memory cache provider. Bound from <c>PowerFeatures:Cache</c> so the
/// provider stays decoupled from the ASP.NET feature-module options type.
/// </summary>
public sealed class BitFasterCacheOptions
{
    /// <summary>Maximum number of in-memory entries.</summary>
    public int Capacity { get; set; } = 1000;
}

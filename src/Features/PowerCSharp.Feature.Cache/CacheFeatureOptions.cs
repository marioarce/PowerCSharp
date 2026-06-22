using PowerCSharp.Feature.Cache.Abstractions.Enums;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Feature.Cache;

/// <summary>Options for the Cache feature, bound from <c>PowerFeatures:Cache</c>.</summary>
public sealed class CacheFeatureOptions : FeatureOptionsBase
{
    /// <summary>The backend to use. The variant flag drives selection.</summary>
    public CacheProvider Provider { get; set; } = CacheProvider.None;

    /// <summary>Maximum number of in-memory entries.</summary>
    public int Capacity { get; set; } = 1000;
}

using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Feature.Cache;

/// <summary>Options for the Cache feature, bound from <c>PowerFeatures:Cache</c>.</summary>
public sealed class CacheFeatureOptions : FeatureOptionsBase
{
    /// <summary>The backend to use. The variant flag drives selection.</summary>
    public CacheProvider Provider { get; set; } = CacheProvider.None;

    /// <summary>Maximum number of in-memory entries.</summary>
    public int Capacity { get; set; } = 1000;

    /// <summary>Disk-cache settings.</summary>
    public DiskCacheOptions Disk { get; set; } = new();
}

/// <summary>Disk-cache settings for the Cache feature.</summary>
public sealed class DiskCacheOptions
{
    /// <summary>Whether the disk cache is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Directory used to store disk-cache entries.</summary>
    public string? DirectoryPath { get; set; }

    /// <summary>Default time-to-live in seconds for disk entries (0 = no expiry).</summary>
    public int DefaultTtlSeconds { get; set; }
}

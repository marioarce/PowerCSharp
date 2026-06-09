using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Registry;

/// <summary>
/// A snapshot of a single discovered feature's resolved state, captured at registration.
/// </summary>
public sealed class FeatureRegistryEntry
{
    /// <summary>The feature key.</summary>
    public required string Key { get; init; }

    /// <summary>The feature tier.</summary>
    public FeatureTier Tier { get; init; }

    /// <summary>Registration/middleware order.</summary>
    public int Order { get; init; }

    /// <summary>Whether the feature resolved as enabled.</summary>
    public bool Enabled { get; init; }

    /// <summary>The source that produced the resolved flag value.</summary>
    public FeatureFlagSource Source { get; init; }

    /// <summary>The package that shipped the feature, when known.</summary>
    public string? PackageId { get; init; }

    /// <summary>The feature/package version, when known.</summary>
    public string? Version { get; init; }
}

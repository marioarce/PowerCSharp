namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// Metadata describing a feature, used by the engine's registry and diagnostics.
/// </summary>
public sealed class FeatureDescriptor
{
    /// <summary>Stable identifier (e.g. <c>"Cache"</c>); matches the config section and flag key.</summary>
    public required string Key { get; init; }

    /// <summary>Human-friendly name for diagnostics; defaults to <see cref="Key"/> when unset.</summary>
    public string? DisplayName { get; init; }

    /// <summary>The tier this feature belongs to.</summary>
    public FeatureTier Tier { get; init; }

    /// <summary>The flag state used when no source supplies a value.</summary>
    public bool DefaultEnabled { get; init; }

    /// <summary>The NuGet package id that ships the feature, when applicable.</summary>
    public string? PackageId { get; init; }

    /// <summary>The feature/package version, when applicable.</summary>
    public string? Version { get; init; }
}

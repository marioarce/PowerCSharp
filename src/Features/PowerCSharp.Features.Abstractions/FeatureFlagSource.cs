namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// Indicates where a resolved feature-flag value originated, in precedence order
/// (highest wins). Surfaced in diagnostics so consumers can see why a feature is on or off.
/// </summary>
public enum FeatureFlagSource
{
    /// <summary>No value was found; the feature's declared default was used.</summary>
    Default = 0,

    /// <summary>Resolved from <c>appsettings</c> (<c>PowerFeatures:&lt;Key&gt;</c>).</summary>
    Configuration = 1,

    /// <summary>Resolved from an environment variable (<c>POWERFEATURES__&lt;KEY&gt;__ENABLED</c>).</summary>
    Environment = 2,

    /// <summary>Resolved from a custom/advanced <see cref="IFeatureFlagProvider"/>.</summary>
    Provider = 3,

    /// <summary>Resolved from an explicit code override supplied by the host.</summary>
    Override = 4,
}

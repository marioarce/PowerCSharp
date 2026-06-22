namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// Base type for a feature's strongly-typed options, bound from its configuration section
/// (<c>PowerFeatures:&lt;Key&gt;</c>). Always carries the <see cref="Enabled"/> flag.
/// </summary>
public abstract class FeatureOptionsBase
{
    /// <summary>Whether the feature is enabled. May be overridden by higher-precedence flag sources.</summary>
    public bool Enabled { get; set; }
}

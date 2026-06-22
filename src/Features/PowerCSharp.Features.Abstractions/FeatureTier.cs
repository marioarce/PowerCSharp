namespace PowerCSharp.Features.Abstractions;

/// <summary>
/// Identifies which tier a feature belongs to.
/// </summary>
public enum FeatureTier
{
    /// <summary>
    /// Group 1 — lightweight capability shipped in the <c>PowerCSharp.BuiltInFeatures</c> bundle,
    /// gated by runtime flag only.
    /// </summary>
    BuiltIn = 0,

    /// <summary>
    /// Group 2 — self-contained, possibly third-party-bearing capability shipped as its own
    /// package (or family), gated by both package reference and runtime flag.
    /// </summary>
    Pluggable = 1,
}

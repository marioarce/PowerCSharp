using PowerCSharp.BuiltInFeatures.Cors;
using PowerCSharp.Features;

namespace PowerCSharp.BuiltInFeatures;

/// <summary>
/// Convenience registration for the Built-in Features bundle.
/// </summary>
public static class BuiltInFeaturesOptionsExtensions
{
    /// <summary>
    /// Opts the Built-in Features bundle assembly in to auto-discovery, so all built-in
    /// modules (e.g. CORS) are discovered and runtime-flag toggled.
    /// </summary>
    public static PowerFeaturesOptions AddBuiltInFeatures(this PowerFeaturesOptions options)
        => options.ScanAssemblies(typeof(CorsFeatureModule).Assembly);
}

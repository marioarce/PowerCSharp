using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.BuiltInFeatures.Cors;

/// <summary>
/// Options for the built-in CORS feature, bound from <c>PowerFeatures:Cors</c>.
/// </summary>
public sealed class CorsFeatureOptions : FeatureOptionsBase
{
    /// <summary>The CORS policy name registered and applied by the feature.</summary>
    public string PolicyName { get; set; } = "PowerFeaturesCors";

    /// <summary>Explicit allowed origins. When empty (or <see cref="AllowAnyOrigin"/> is true), any origin is allowed.</summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>Whether to allow any origin. Defaults to true when no origins are configured.</summary>
    public bool AllowAnyOrigin { get; set; }

    /// <summary>Whether to allow any request header.</summary>
    public bool AllowAnyHeader { get; set; } = true;

    /// <summary>Whether to allow any HTTP method.</summary>
    public bool AllowAnyMethod { get; set; } = true;

    /// <summary>Whether to allow credentials. Ignored when any origin is allowed (incompatible per CORS spec).</summary>
    public bool AllowCredentials { get; set; }
}

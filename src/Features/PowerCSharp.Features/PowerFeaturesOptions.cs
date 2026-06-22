using System.Reflection;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features;

/// <summary>
/// Configures the Features engine. Supplied via the <c>AddPowerFeatures</c> callback.
/// All members are fluent.
/// </summary>
public sealed class PowerFeaturesOptions
{
    internal List<Assembly> Assemblies { get; } = new();

    internal List<IFeatureModule> Modules { get; } = new();

    internal Dictionary<string, string> Overrides { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal List<IFeatureFlagProvider> AdditionalProviders { get; } = new();

    /// <summary>Whether the diagnostics HTTP endpoint is enabled (off by default).</summary>
    public bool DiagnosticsEndpointEnabled { get; private set; }

    /// <summary>The path the diagnostics endpoint is served at.</summary>
    public string DiagnosticsPath { get; private set; } = "/power-features";

    /// <summary>Opts the supplied assemblies in to auto-discovery of <see cref="IFeatureModule"/>s.</summary>
    public PowerFeaturesOptions ScanAssemblies(params Assembly[] assemblies)
    {
        Assemblies.AddRange(assemblies);
        return this;
    }

    /// <summary>Registers an explicit module instance (no reflection required).</summary>
    public PowerFeaturesOptions AddModule(IFeatureModule module)
    {
        Modules.Add(module);
        return this;
    }

    /// <summary>Sets an explicit boolean override (highest precedence) for a feature key.</summary>
    public PowerFeaturesOptions Override(string featureKey, bool enabled)
    {
        Overrides[featureKey] = enabled ? "true" : "false";
        return this;
    }

    /// <summary>Sets an explicit variant override (highest precedence) for a feature key.</summary>
    public PowerFeaturesOptions Override(string featureKey, string value)
    {
        Overrides[featureKey] = value;
        return this;
    }

    /// <summary>Adds a custom/advanced flag provider (e.g. secret-driven, App Config).</summary>
    public PowerFeaturesOptions AddFlagProvider(IFeatureFlagProvider provider)
    {
        AdditionalProviders.Add(provider);
        return this;
    }

    /// <summary>Enables the opt-in diagnostics HTTP endpoint, optionally at a custom path.</summary>
    public PowerFeaturesOptions EnableDiagnosticsEndpoint(string? path = null)
    {
        DiagnosticsEndpointEnabled = true;
        if (!string.IsNullOrWhiteSpace(path))
        {
            DiagnosticsPath = path!;
        }

        return this;
    }
}

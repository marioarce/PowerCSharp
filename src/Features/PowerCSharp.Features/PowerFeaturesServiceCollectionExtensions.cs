using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PowerCSharp.Features.Abstractions;
using PowerCSharp.Features.Flags;
using PowerCSharp.Features.Internal;
using PowerCSharp.Features.Registry;

namespace PowerCSharp.Features;

/// <summary>Host integration entry point for registering PowerCSharp features.</summary>
public static class PowerFeaturesServiceCollectionExtensions
{
    /// <summary>
    /// Builds the composite flag resolver, discovers feature modules from opted-in assemblies
    /// (plus explicit modules), invokes each module's <c>ConfigureServices</c> (modules self-gate
    /// on their flag), and records a <see cref="FeatureRegistry"/> for diagnostics.
    /// </summary>
    public static IServiceCollection AddPowerFeatures(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<PowerFeaturesOptions>? configure = null)
    {
        var options = new PowerFeaturesOptions();
        configure?.Invoke(options);

        // Compose providers in precedence order (highest first):
        // Override > custom providers > environment > configuration.
        var providers = new List<IFeatureFlagProvider>();
        if (options.Overrides.Count > 0)
        {
            providers.Add(new OverrideFeatureFlagProvider(options.Overrides));
        }

        providers.AddRange(options.AdditionalProviders);
        providers.Add(new EnvironmentFeatureFlagProvider());
        providers.Add(new ConfigurationFeatureFlagProvider(configuration));

        var flags = new CompositeFeatureFlagProvider(providers);
        services.AddSingleton<IFeatureFlagProvider>(flags);

        var modules = FeatureModuleDiscovery.Discover(options.Assemblies, options.Modules);

        var registry = new FeatureRegistry();
        var moduleEntries = new List<FeatureModuleEntry>(modules.Count);

        foreach (var module in modules)
        {
            var descriptor = BuildDescriptor(module);
            var value = flags.GetValue(module.FeatureKey);
            var enabled = value.HasValue ? value.AsBoolean(descriptor.DefaultEnabled) : descriptor.DefaultEnabled;

            registry.Add(new FeatureRegistryEntry
            {
                Key = descriptor.Key,
                Tier = descriptor.Tier,
                Order = module.Order,
                Enabled = enabled,
                Source = value.Source,
                PackageId = descriptor.PackageId,
                Version = descriptor.Version,
            });

            // Model A: always invoke ConfigureServices; the module self-gates (active vs NoOp).
            var context = new FeatureRegistrationContext(
                services,
                configuration,
                flags,
                NullLogger.Instance,
                descriptor);

            module.ConfigureServices(context);
            moduleEntries.Add(new FeatureModuleEntry(module, descriptor));
        }

        services.AddSingleton(registry);
        services.AddSingleton(new FeaturePipelinePlan
        {
            Modules = moduleEntries,
            DiagnosticsEndpointEnabled = options.DiagnosticsEndpointEnabled,
            DiagnosticsPath = options.DiagnosticsPath,
        });

        return services;
    }

    private static FeatureDescriptor BuildDescriptor(IFeatureModule module)
    {
        var assemblyName = module.GetType().Assembly.GetName();
        var packageId = assemblyName.Name;
        var tier = packageId is not null && packageId.Contains("BuiltInFeatures", StringComparison.OrdinalIgnoreCase)
            ? FeatureTier.BuiltIn
            : FeatureTier.Pluggable;

        return new FeatureDescriptor
        {
            Key = module.FeatureKey,
            DisplayName = module.FeatureKey,
            Tier = tier,
            DefaultEnabled = false,
            PackageId = packageId,
            Version = assemblyName.Version?.ToString(),
        };
    }
}

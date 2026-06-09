using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Cache.NoOp;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Feature.Cache;

/// <summary>
/// Cache feature module. Binds options and registers a NoOp floor so dependents always resolve.
/// A provider package (e.g. BitFaster) registers a concrete implementation that overrides the NoOp
/// when the feature is enabled and a provider is selected. Supports auto-discovery and the explicit
/// <see cref="CacheFeatureExtensions.AddCacheFeature"/>.
/// </summary>
public sealed class CacheFeatureModule : IFeatureModule
{
    /// <summary>The feature key.</summary>
    public const string Key = "Cache";

    /// <inheritdoc />
    public string FeatureKey => Key;

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public void ConfigureServices(IFeatureRegistrationContext context)
    {
        context.Services.Configure<CacheFeatureOptions>(
            context.Configuration.GetSection($"PowerFeatures:{Key}"));

        if (!context.Flags.IsEnabled(FeatureKey))
        {
            context.Logger.LogInformation("Cache feature disabled; registering NoOp cache services.");
        }

        // Safe-off floor: NoOp is registered unless a provider package supplies a concrete
        // implementation (providers use plain Add, which takes precedence over these TryAdds).
        context.Services.TryAddSingleton<ICacheService, NoOpCacheService>();
        context.Services.TryAddSingleton<IDiskCacheService, NoOpDiskCacheService>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;
using PowerCSharp.Features.Abstractions;
using System.Reflection;
using System;

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
        // Force-load the abstractions assembly so the CLR can resolve its types during discovery.
        _ = Assembly.Load("PowerCSharp.Feature.Cache.Abstractions");

        context.Services.Configure<CacheFeatureOptions>(
            context.Configuration.GetSection($"PowerFeatures:{Key}"));

        // NOTE: context.Logger is NullLogger at this stage (the DI container isn't built yet, so
        // no real ILoggerFactory exists). Anything logged here is silently discarded. The
        // authoritative "which implementation actually won" diagnostic is logged for real in
        // ConfigurePipeline below, once a real logger is available.
        if (!context.Flags.IsEnabled(FeatureKey))
        {
            context.Logger.LogInformation("Cache feature disabled; registering NoOp cache services.");
        }

        // Safe-off floor: NoOp is registered unless a provider package supplies a concrete
        // implementation (providers use plain Add, which takes precedence over these TryAdds).
        context.Services.TryAddSingleton<ICacheService, NoOpCacheService>();
        context.Services.TryAddSingleton<IDiskCacheService, NoOpDiskCacheService>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Diagnostic-only: resolves the services actually bound to <see cref="ICacheService"/> and
    /// <see cref="IDiskCacheService"/> from the fully-built container and logs their concrete
    /// types with a real logger. This is the authoritative answer to "which cache implementation
    /// is actually active" — unlike constructor-time logs from individual implementations (which
    /// can fire for shadowed/overridden registrations too, e.g. under ASP.NET Core's
    /// ValidateOnBuild in Development, without ever being the instance that's actually injected).
    /// </remarks>
    public void ConfigurePipeline(IFeaturePipelineContext context)
    {
        var services = context.App.ApplicationServices;
        var cache = services.GetRequiredService<ICacheService>();
        var diskCache = services.GetRequiredService<IDiskCacheService>();

        context.Logger.LogInformation(
            "Cache feature resolved -> ICacheService: {CacheImplementation}; IDiskCacheService: {DiskCacheImplementation}. " +
            "A NoOp implementation here means no provider package overrode the safe-off floor for that contract.",
            cache.GetType().FullName,
            diskCache.GetType().FullName);
    }
}

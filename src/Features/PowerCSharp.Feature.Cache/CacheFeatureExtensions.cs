using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;

namespace PowerCSharp.Feature.Cache;

/// <summary>Explicit (no-reflection) registration for the Cache feature contracts and options.</summary>
public static class CacheFeatureExtensions
{
    /// <summary>
    /// Binds <see cref="CacheFeatureOptions"/> from <c>PowerFeatures:Cache</c> and registers the
    /// NoOp safe-off floor. Pair with a provider package's registration to choose a backend.
    /// </summary>
    public static IServiceCollection AddCacheFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheFeatureOptions>(configuration.GetSection($"PowerFeatures:{CacheFeatureModule.Key}"));
        services.TryAddSingleton<ICacheService, NoOpCacheService>();
        services.TryAddSingleton<IDiskCacheService, NoOpDiskCacheService>();
        return services;
    }
}

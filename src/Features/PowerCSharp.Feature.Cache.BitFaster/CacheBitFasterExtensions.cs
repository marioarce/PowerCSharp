using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Cache;

namespace PowerCSharp.Feature.Cache.BitFaster;

/// <summary>
/// Registers the BitFaster-backed cache implementations. Called by the host when it chooses the
/// BitFaster provider (<c>PowerFeatures:Cache:Provider = BitFaster</c>). BitFaster types are only
/// referenced from this package.
/// </summary>
public static class CacheBitFasterExtensions
{
    /// <summary>
    /// Binds <see cref="CacheFeatureOptions"/> and registers the BitFaster cache and file disk cache,
    /// overriding the NoOp floor registered by the contracts package.
    /// </summary>
    public static IServiceCollection AddCacheBitFaster(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheFeatureOptions>(configuration.GetSection($"PowerFeatures:{CacheFeatureModule.Key}"));

        // Plain Add takes precedence over the contracts package's TryAdd NoOp registration.
        services.AddSingleton<ICacheService, BitFasterCacheService>();
        services.AddSingleton<IDiskCacheService, FileDiskCacheService>();
        return services;
    }
}

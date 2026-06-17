using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PowerCSharp.Feature.Cache.BitFaster;

/// <summary>
/// Registers the BitFaster-backed in-memory cache. Called by the host when it chooses the
/// BitFaster provider (<c>PowerFeatures:Cache:Provider = BitFaster</c>). BitFaster types are only
/// referenced from this package. Disk caching is provided separately by
/// <c>PowerCSharp.Feature.Cache.Disk</c>.
/// </summary>
public static class CacheBitFasterExtensions
{
    /// <summary>
    /// Binds <see cref="BitFasterCacheOptions"/> from <c>PowerFeatures:Cache</c> and registers the
    /// BitFaster in-memory cache, overriding the NoOp floor registered by the contracts package.
    /// </summary>
    public static IServiceCollection AddCacheBitFaster(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BitFasterCacheOptions>(configuration.GetSection("PowerFeatures:Cache"));

        // Plain Add takes precedence over the contracts package's TryAdd NoOp registration.
        services.AddSingleton<ICacheService, BitFasterCacheService>();
        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Registers the disk-backed cache provider. Called by the host when disk caching is enabled.
/// </summary>
public static class CacheDiskExtensions
{
    /// <summary>
    /// Binds <see cref="DiskCacheOptions"/> from <c>PowerFeatures:Cache:Disk</c> and registers the
    /// disk cache, overriding the NoOp floor registered by the contracts package.
    /// </summary>
    public static IServiceCollection AddCacheDisk(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DiskCacheOptions>(configuration.GetSection("PowerFeatures:Cache:Disk"));

        // Register concrete class first for hosted service dependency
        services.AddSingleton<DiskCacheService>();
        
        // Then register interface mapping
        services.AddSingleton<IDiskCacheService>(provider => provider.GetRequiredService<DiskCacheService>());

#if NET8_0_OR_GREATER
        // Register the hosted-service wrapper on net8.0 only for host lifecycle integration.
        services.AddHostedService<DiskCacheBackgroundService>();
#endif

        return services;
    }
}

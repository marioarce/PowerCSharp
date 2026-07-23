using PowerCSharp.Feature.Cache;
using PowerCSharp.Feature.Cache.BitFaster;
using PowerCSharp.Feature.Sanitization;
using PowerCSharp.Features;

namespace WebSample.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Swagger services to the service collection
    /// </summary>
    /// <param name="services">The IServiceCollection instance</param>
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "PowerCSharp Web Sample API", Version = "v1" });

            // Include XML comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    /// <summary>
    /// Registers the PowerCSharp Features Framework via auto-discovery, opting in the Cache and
    /// Sanitization feature modules. Also registers the BitFaster cache provider so the Cache demo
    /// endpoint has an active backend rather than the NoOp floor.
    /// </summary>
    /// <param name="services">The IServiceCollection instance</param>
    /// <param name="configuration">The application configuration</param>
    public static IServiceCollection AddPowerCSharpFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPowerFeatures(configuration, options =>
        {
            options.ScanAssemblies(
                typeof(CacheFeatureModule).Assembly,
                typeof(SanitizationFeatureModule).Assembly);
        });

        services.AddCacheBitFaster(configuration);

        return services;
    }
}

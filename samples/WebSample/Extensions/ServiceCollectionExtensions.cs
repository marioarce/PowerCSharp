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
}

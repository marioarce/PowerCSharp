using WebSample.Samples.Core;
using WebSample.Samples.Extensions;
using WebSample.Samples.Helpers;
using WebSample.Samples.Utilities;

namespace WebSample.Extensions;

/// <summary>
/// Extension methods for WebApplication to register PowerCSharp demo endpoints
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures Swagger UI middleware
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    public static void UseSwaggerUI(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PowerCSharp Web Sample API v1");
            c.RoutePrefix = "swagger";
        });
    }

    /// <summary>
    /// Maps all PowerCSharp demo endpoints using dedicated endpoint classes
    /// </summary>
    /// <param name="app">The WebApplication instance</param>
    public static void MapPowerCSharpDemoEndpoints(this WebApplication app)
    {
        // Root endpoint - redirect to Swagger UI
        app.MapGet("/", () => Results.Redirect("/swagger"));

        // Core Samples
        app.MapGet("/demo/string", StringSampleEndpoints.GetDemoData);
        app.MapGet("/demo/validation", ValidationSampleEndpoints.GetDemoData);
        app.MapGet("/demo/json", JsonSampleEndpoints.GetDemoData);

        // Extension Samples
        app.MapGet("/demo/compression", CompressionSampleEndpoints.GetDemoData);
        app.MapGet("/demo/http", HttpSampleEndpoints.GetDemoData);
        app.MapGet("/demo/datetime", DateTimeSampleEndpoints.GetDemoData);
        app.MapGet("/demo/hash", HashSampleEndpoints.GetDemoData);
        app.MapGet("/demo/path", PathSampleEndpoints.GetDemoData);

        // Helper Samples
        app.MapGet("/demo/crypto", CryptoSampleEndpoints.GetDemoData);
        app.MapGet("/demo/environment", EnvironmentSampleEndpoints.GetDemoData);
        app.MapGet("/demo/async", AsyncSampleEndpoints.GetDemoData);

        // Utility Samples
        app.MapGet("/demo/math", MathSampleEndpoints.GetDemoData);
        app.MapGet("/demo/collection", CollectionSampleEndpoints.GetDemoData);
        app.MapGet("/demo/dictionary", DictionarySampleEndpoints.GetDemoData);
        app.MapGet("/demo/unix-timestamp", UnixTimestampSampleEndpoints.GetDemoData);
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerCSharp.Features.Abstractions;
using PowerCSharp.Features.Diagnostics;
using PowerCSharp.Features.Internal;
using PowerCSharp.Features.Registry;

namespace PowerCSharp.Features;

/// <summary>Host integration entry point for applying enabled features' middleware.</summary>
public static class PowerFeaturesApplicationBuilderExtensions
{
    /// <summary>
    /// Logs the resolved feature matrix and invokes <c>ConfigurePipeline</c> for each enabled,
    /// middleware-bearing feature in ascending <c>Order</c>. Optionally maps the diagnostics endpoint.
    /// </summary>
    public static IApplicationBuilder UsePowerFeatures(this IApplicationBuilder app)
    {
        var services = app.ApplicationServices;
        var plan = services.GetRequiredService<FeaturePipelinePlan>();
        var registry = services.GetRequiredService<FeatureRegistry>();
        var flags = services.GetRequiredService<IFeatureFlagProvider>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("PowerCSharp.Features");

        FeatureDiagnostics.LogMatrix(logger, registry);

        foreach (var entry in plan.Modules)
        {
            if (!flags.IsEnabled(entry.Module.FeatureKey))
            {
                continue;
            }

            var context = new FeaturePipelineContext(
                app,
                configuration,
                flags,
                loggerFactory.CreateLogger($"PowerFeature.{entry.Module.FeatureKey}"),
                entry.Descriptor);

            entry.Module.ConfigurePipeline(context);
        }

        if (FeatureDiagnostics.IsEndpointEnabled(plan, configuration))
        {
            FeatureDiagnostics.MapEndpoint(app, plan.DiagnosticsPath, registry);
        }

        return app;
    }
}

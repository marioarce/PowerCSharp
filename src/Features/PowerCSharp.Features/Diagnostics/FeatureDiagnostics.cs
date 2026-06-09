using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerCSharp.Features.Flags;
using PowerCSharp.Features.Internal;
using PowerCSharp.Features.Registry;

namespace PowerCSharp.Features.Diagnostics;

/// <summary>Startup logging and the opt-in HTTP endpoint for the resolved feature matrix.</summary>
internal static class FeatureDiagnostics
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void LogMatrix(ILogger logger, FeatureRegistry registry)
    {
        if (registry.Entries.Count == 0)
        {
            logger.LogInformation("PowerFeatures: no features discovered.");
            return;
        }

        foreach (var entry in registry.Entries)
        {
            logger.LogInformation(
                "PowerFeature {Key} [{Tier}] enabled={Enabled} source={Source} order={Order} package={PackageId}",
                entry.Key,
                entry.Tier,
                entry.Enabled,
                entry.Source,
                entry.Order,
                entry.PackageId ?? "(unknown)");
        }
    }

    public static bool IsEndpointEnabled(FeaturePipelinePlan plan, IConfiguration configuration)
    {
        if (plan.DiagnosticsEndpointEnabled)
        {
            return true;
        }

        var configured = configuration[$"{ConfigurationFeatureFlagProvider.SectionRoot}:Diagnostics:Enabled"];
        return bool.TryParse(configured, out var enabled) && enabled;
    }

    public static void MapEndpoint(IApplicationBuilder app, string path, FeatureRegistry registry)
    {
        app.Map(path, branch => branch.Run(async context =>
        {
            context.Response.ContentType = "application/json";

            var payload = registry.Entries.Select(e => new
            {
                e.Key,
                Tier = e.Tier.ToString(),
                e.Order,
                e.Enabled,
                Source = e.Source.ToString(),
                e.PackageId,
                e.Version,
            });

            await context.Response
                .WriteAsync(JsonSerializer.Serialize(payload, JsonOptions))
                .ConfigureAwait(false);
        }));
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.BuiltInFeatures.Cors;

/// <summary>
/// Built-in CORS feature. Registers a named CORS policy from <c>PowerFeatures:Cors</c> and applies
/// it to the pipeline when enabled. CORS has no resolvable dependents, so the flag-off path simply
/// skips registration (no NoOp required).
/// </summary>
public sealed class CorsFeatureModule : IFeatureModule
{
    /// <summary>The feature key.</summary>
    public const string Key = "Cors";

    /// <inheritdoc />
    public string FeatureKey => Key;

    /// <inheritdoc />
    public int Order => 10;

    /// <inheritdoc />
    public void ConfigureServices(IFeatureRegistrationContext context)
    {
        if (!context.Flags.IsEnabled(FeatureKey))
        {
            return;
        }

        var options = ReadOptions(context.Configuration);

        context.Services.AddCors(cors => cors.AddPolicy(options.PolicyName, policy =>
        {
            if (options.AllowAnyHeader)
            {
                policy.AllowAnyHeader();
            }

            if (options.AllowAnyMethod)
            {
                policy.AllowAnyMethod();
            }

            var anyOrigin = options.AllowAnyOrigin || options.AllowedOrigins.Length == 0;
            if (anyOrigin)
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(options.AllowedOrigins);

                // AllowCredentials is incompatible with AllowAnyOrigin per the CORS spec.
                if (options.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            }
        }));
    }

    /// <inheritdoc />
    public void ConfigurePipeline(IFeaturePipelineContext context)
    {
        var options = ReadOptions(context.Configuration);
        context.App.UseCors(options.PolicyName);
    }

    private static CorsFeatureOptions ReadOptions(IConfiguration configuration)
        => configuration.GetSection($"PowerFeatures:{Key}").Get<CorsFeatureOptions>() ?? new CorsFeatureOptions();
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Feature.Sanitization.Abstractions.NoOp;
using PowerCSharp.Features.Abstractions;
using System.Reflection;

namespace PowerCSharp.Feature.Sanitization;

/// <summary>
/// Sanitization feature module. Binds options and registers either the real
/// <see cref="SanitizationService"/> (feature enabled) or <see cref="NoOpSanitizationService"/>
/// (feature disabled). Unlike the Cache feature family, Sanitization has no swappable-backend
/// provider package, so this module registers the concrete implementation directly rather than
/// deferring to one. Supports auto-discovery and the explicit
/// <see cref="SanitizationFeatureExtensions.AddSanitizationFeature"/>.
/// </summary>
public sealed class SanitizationFeatureModule : IFeatureModule
{
    /// <summary>The feature key.</summary>
    public const string Key = "Sanitization";

    /// <inheritdoc />
    public string FeatureKey => Key;

    /// <inheritdoc />
    public int Order => 20;

    /// <inheritdoc />
    public void ConfigureServices(IFeatureRegistrationContext context)
    {
        // Force-load the abstractions assembly so the CLR can resolve its types during discovery.
        _ = Assembly.Load("PowerCSharp.Feature.Sanitization.Abstractions");

        context.Services.Configure<SanitizationFeatureOptions>(
            context.Configuration.GetSection($"PowerFeatures:{Key}"));

        if (!context.Flags.IsEnabled(FeatureKey))
        {
            context.Logger.LogInformation("Sanitization feature disabled; registering NoOp sanitization service.");
            context.Services.TryAddSingleton<ISanitizationService, NoOpSanitizationService>();
            return;
        }

        context.Services.TryAddSingleton<ISanitizationSettingsProvider, SanitizationSettingsProvider>();
        context.Services.TryAddSingleton<ISanitizationService, SanitizationService>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Contributes no middleware. Used solely to bridge the DI-resolved
    /// <see cref="ISanitizationSettingsProvider"/> into the static <see cref="SanitizationEngine"/>
    /// once the application's <see cref="IServiceProvider"/> has been built, so extension-method
    /// call sites (e.g. <c>input.SanitizeForLog()</c>) reflect the host's configuration too.
    /// </remarks>
    public void ConfigurePipeline(IFeaturePipelineContext context)
    {
        context.App.ApplicationServices.ConfigureSanitizationEngine();
    }
}

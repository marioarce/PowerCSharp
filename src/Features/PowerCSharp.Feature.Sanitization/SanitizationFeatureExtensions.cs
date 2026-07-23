using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PowerCSharp.Feature.Sanitization.Abstractions;

namespace PowerCSharp.Feature.Sanitization;

/// <summary>Explicit (no-reflection) registration for the Sanitization feature contracts and options.</summary>
public static class SanitizationFeatureExtensions
{
    /// <summary>
    /// Binds <see cref="SanitizationFeatureOptions"/> from <c>PowerFeatures:Sanitization</c> and
    /// registers the real <see cref="SanitizationService"/>. Unlike
    /// <c>CacheFeatureExtensions.AddCacheFeature</c>, this registers the concrete implementation
    /// directly rather than a NoOp floor: Sanitization has no separate provider package to defer
    /// to, so explicit opt-in here means the caller wants sanitization active. Call
    /// <see cref="SanitizationEngineServiceProviderExtensions.ConfigureSanitizationEngine"/> on the
    /// built <see cref="IServiceProvider"/> to also wire the static
    /// <see cref="SanitizationEngine"/> for non-DI call sites.
    /// </summary>
    public static IServiceCollection AddSanitizationFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SanitizationFeatureOptions>(
            configuration.GetSection($"PowerFeatures:{SanitizationFeatureModule.Key}"));

        services.TryAddSingleton<ISanitizationSettingsProvider, SanitizationSettingsProvider>();
        services.TryAddSingleton<ISanitizationService, SanitizationService>();

        return services;
    }
}

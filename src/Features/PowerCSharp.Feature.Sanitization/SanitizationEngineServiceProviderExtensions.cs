using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Sanitization.Abstractions;

namespace PowerCSharp.Feature.Sanitization;

/// <summary>
/// Wires a built <see cref="IServiceProvider"/>'s <see cref="ISanitizationSettingsProvider"/> into
/// the static <see cref="SanitizationEngine"/>, so call sites that use the engine directly (e.g.
/// <c>SanitizationExtensions.SanitizeForLog</c>) reflect the host's configured settings without
/// needing to resolve <see cref="ISanitizationService"/> from DI.
/// </summary>
public static class SanitizationEngineServiceProviderExtensions
{
    /// <summary>
    /// Resolves <see cref="ISanitizationSettingsProvider"/> from <paramref name="serviceProvider"/>,
    /// if registered, and calls <see cref="SanitizationEngine.SetConfigurationProvider"/> with it.
    /// A no-op when the Sanitization feature is disabled (no settings provider registered) or when
    /// only the explicit <c>AddSanitizationFeature</c> NoOp registration path was used.
    /// </summary>
    /// <param name="serviceProvider">The built application service provider.</param>
    /// <returns>The same <paramref name="serviceProvider"/>, for chaining.</returns>
    public static IServiceProvider ConfigureSanitizationEngine(this IServiceProvider serviceProvider)
    {
        var settingsProvider = serviceProvider.GetService<ISanitizationSettingsProvider>();

        if (settingsProvider is not null)
        {
            SanitizationEngine.SetConfigurationProvider(settingsProvider.GetCurrentSettings);
        }

        return serviceProvider;
    }
}

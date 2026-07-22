namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Provides the current <see cref="SanitizationSettings"/> to the static <see cref="SanitizationEngine"/>.
/// Allows a hosting layer to bridge its own configuration/options system into the engine via
/// <see cref="SanitizationEngine.SetConfigurationProvider"/> without the engine taking a direct
/// dependency on any specific configuration framework.
/// </summary>
public interface ISanitizationSettingsProvider
{
    /// <summary>
    /// Gets the current sanitization settings from configuration.
    /// </summary>
    /// <returns>The current sanitization settings.</returns>
    SanitizationSettings GetCurrentSettings();
}

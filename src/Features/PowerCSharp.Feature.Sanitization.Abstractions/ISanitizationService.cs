namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Defines the contract for a configuration-aware sanitization service, suitable for dependency
/// injection. Implementations wrap the static <see cref="SanitizationEngine"/> while sourcing
/// settings from the host's configuration system.
/// </summary>
public interface ISanitizationService
{
    /// <summary>
    /// Sanitizes a string to prevent log injection attacks by removing or encoding control characters.
    /// This method is optimized for performance and never throws exceptions.
    /// Uses the service's current configuration.
    /// </summary>
    /// <param name="input">The input string to sanitize. Can be null.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the sanitized string and operation details.</returns>
    SanitizationResult SanitizeForLogInjection(string? input);

    /// <summary>
    /// Sanitizes a string to prevent log injection attacks using specific settings.
    /// This method is optimized for performance and never throws exceptions.
    /// </summary>
    /// <param name="input">The input string to sanitize. Can be null.</param>
    /// <param name="settings">Specific sanitization settings to use. If null, uses the service's current configuration.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the sanitized string and operation details.</returns>
    SanitizationResult SanitizeForLogInjection(string? input, SanitizationSettings? settings);

    /// <summary>
    /// Sanitizes a file path to prevent directory traversal attacks (CWE-22).
    /// Uses the service's current configuration.
    /// </summary>
    /// <param name="input">The input file path to sanitize. Can be null.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated file path and operation details.</returns>
    SanitizationResult SanitizeForFilePath(string? input);

    /// <summary>
    /// Sanitizes a file path to prevent directory traversal attacks (CWE-22) using specific settings.
    /// </summary>
    /// <param name="input">The input file path to sanitize. Can be null.</param>
    /// <param name="settings">Specific sanitization settings to use. If null, uses the service's current configuration.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated file path and operation details.</returns>
    SanitizationResult SanitizeForFilePath(string? input, SanitizationSettings? settings);

    /// <summary>
    /// Detects and masks sensitive data in a string to prevent information disclosure (CWE-200).
    /// Uses the service's current configuration.
    /// </summary>
    /// <param name="input">The input string to check for sensitive data. Can be null.</param>
    /// <returns>A <see cref="SensitiveDataResult"/> containing the masked string and operation details.</returns>
    SensitiveDataResult SanitizeForSensitiveData(string? input);

    /// <summary>
    /// Detects and masks sensitive data in a string using specific settings.
    /// </summary>
    /// <param name="input">The input string to check for sensitive data. Can be null.</param>
    /// <param name="settings">Specific sanitization settings to use. If null, uses the service's current configuration.</param>
    /// <returns>A <see cref="SensitiveDataResult"/> containing the masked string and operation details.</returns>
    SensitiveDataResult SanitizeForSensitiveData(string? input, SanitizationSettings? settings);

    /// <summary>
    /// Validates a regex pattern for safety against ReDoS attacks (CWE-400/CWE-730).
    /// Uses the service's current configuration.
    /// </summary>
    /// <param name="pattern">The regex pattern to validate. Can be null.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated pattern and operation details.</returns>
    SanitizationResult SanitizeForRegexInjection(string? pattern);

    /// <summary>
    /// Validates a regex pattern for safety against ReDoS attacks (CWE-400/CWE-730) using specific settings.
    /// </summary>
    /// <param name="pattern">The regex pattern to validate. Can be null.</param>
    /// <param name="settings">Specific sanitization settings to use. If null, uses the service's current configuration.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated pattern and operation details.</returns>
    SanitizationResult SanitizeForRegexInjection(string? pattern, SanitizationSettings? settings);

    /// <summary>
    /// Gets the current sanitization configuration used by this service.
    /// </summary>
    /// <returns>The current <see cref="SanitizationSettings"/>.</returns>
    SanitizationSettings GetCurrentSettings();
}

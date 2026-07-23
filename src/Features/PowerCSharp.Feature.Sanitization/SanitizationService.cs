using PowerCSharp.Feature.Sanitization.Abstractions;

namespace PowerCSharp.Feature.Sanitization;

/// <summary>
/// Configuration-aware <see cref="ISanitizationService"/> implementation. Delegates every
/// operation to the static <see cref="SanitizationEngine"/>, sourcing settings from an injected
/// <see cref="ISanitizationSettingsProvider"/> so the options-to-settings mapping lives in exactly
/// one place.
/// </summary>
public sealed class SanitizationService : ISanitizationService
{
    private readonly ISanitizationSettingsProvider _settingsProvider;

    /// <summary>Initializes a new instance of <see cref="SanitizationService"/>.</summary>
    /// <param name="settingsProvider">The settings provider used to source current settings.</param>
    public SanitizationService(ISanitizationSettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
    }

    /// <inheritdoc />
    public SanitizationResult SanitizeForLogInjection(string? input)
        => SanitizationEngine.SanitizeForLogInjection(input, GetCurrentSettings());

    /// <inheritdoc />
    public SanitizationResult SanitizeForLogInjection(string? input, SanitizationSettings? settings)
        => SanitizationEngine.SanitizeForLogInjection(input, settings);

    /// <inheritdoc />
    public SanitizationResult SanitizeForFilePath(string? input)
        => SanitizationEngine.SanitizeForFilePath(input, GetCurrentSettings());

    /// <inheritdoc />
    public SanitizationResult SanitizeForFilePath(string? input, SanitizationSettings? settings)
        => SanitizationEngine.SanitizeForFilePath(input, settings);

    /// <inheritdoc />
    public SensitiveDataResult SanitizeForSensitiveData(string? input)
        => SanitizationEngine.SanitizeForSensitiveData(input, GetCurrentSettings());

    /// <inheritdoc />
    public SensitiveDataResult SanitizeForSensitiveData(string? input, SanitizationSettings? settings)
        => SanitizationEngine.SanitizeForSensitiveData(input, settings);

    /// <inheritdoc />
    public SanitizationResult SanitizeForRegexInjection(string? pattern)
        => SanitizationEngine.SanitizeForRegexInjection(pattern, GetCurrentSettings());

    /// <inheritdoc />
    public SanitizationResult SanitizeForRegexInjection(string? pattern, SanitizationSettings? settings)
        => SanitizationEngine.SanitizeForRegexInjection(pattern, settings);

    /// <inheritdoc />
    public SanitizationSettings GetCurrentSettings() => _settingsProvider.GetCurrentSettings();
}

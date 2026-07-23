using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Sanitization.Abstractions;

namespace PowerCSharp.Feature.Sanitization;

/// <summary>
/// Bridges <see cref="SanitizationFeatureOptions"/> (bound from <c>PowerFeatures:Sanitization</c>)
/// into a <see cref="SanitizationSettings"/> instance for the static
/// <see cref="SanitizationEngine"/>, via <see cref="SanitizationEngine.SetConfigurationProvider"/>.
/// </summary>
/// <remarks>
/// Deliberately takes no <c>ILogger</c> dependency. <see cref="SanitizationEngine"/> can invoke the
/// settings provider from within its own security-event logging path; taking a logger dependency
/// here would risk a recursive logging loop.
/// </remarks>
public sealed class SanitizationSettingsProvider : ISanitizationSettingsProvider
{
    private readonly IOptionsMonitor<SanitizationFeatureOptions> _options;

    /// <summary>Initializes a new instance of <see cref="SanitizationSettingsProvider"/>.</summary>
    /// <param name="options">The monitored Sanitization feature options.</param>
    public SanitizationSettingsProvider(IOptionsMonitor<SanitizationFeatureOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public SanitizationSettings GetCurrentSettings()
    {
        var options = _options.CurrentValue;

        return new SanitizationSettings
        {
            EnableLogSanitization = options.EnableLogSanitization,
            SanitizeUnicodeControlChars = options.SanitizeUnicodeControlChars,
            PreserveTabCharacters = options.PreserveTabCharacters,
            LogSanitizationStrategy = options.LogSanitizationStrategy,
            MaxSanitizedStringLength = options.MaxSanitizedStringLength,

            LogSanitizationFailures = options.LogSanitizationFailures,
            SanitizationFailureLogLevel = options.SanitizationFailureLogLevel,
            LogSecurityEvents = options.LogSecurityEvents,

            EnablePerformanceMonitoring = options.EnablePerformanceMonitoring,
            SlowOperationThresholdMs = options.SlowOperationThresholdMs,

            EnableFilePathSanitization = options.EnableFilePathSanitization,
            EnableParentDirectoryTraversalCheck = options.EnableParentDirectoryTraversalCheck,
            UseStrictValidation = options.UseStrictValidation,
            ValidateWindowsReservedNames = options.ValidateWindowsReservedNames,
            AllowUnicodeCharacters = options.AllowUnicodeCharacters,
            MaxPathSegmentLength = options.MaxPathSegmentLength,
            AllowedBaseDirectories = options.AllowedBaseDirectories,
            AllowedFileExtensions = options.AllowedFileExtensions,

            EnableSensitiveDataDetection = options.EnableSensitiveDataDetection,
            SensitiveDataMaskCharacter = options.SensitiveDataMaskCharacter,
            SensitiveDataDetectionStrictness = options.SensitiveDataDetectionStrictness,

            EnableRegexSanitization = options.EnableRegexSanitization,
            MaxRegexValidationTimeout = options.MaxRegexValidationTimeout,
            MaxRegexPatternLength = options.MaxRegexPatternLength,
            MaxRegexComplexityScore = options.MaxRegexComplexityScore,
            AllowUnicodeCategories = options.AllowUnicodeCategories,
            AllowQuantifiers = options.AllowQuantifiers,
            AllowNestedQuantifiers = options.AllowNestedQuantifiers,
            AllowBackreferences = options.AllowBackreferences,
            AllowLookarounds = options.AllowLookarounds
        };
    }
}

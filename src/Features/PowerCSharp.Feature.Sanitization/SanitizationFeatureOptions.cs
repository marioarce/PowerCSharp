using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Feature.Sanitization;

/// <summary>
/// Options for the Sanitization feature, bound from <c>PowerFeatures:Sanitization</c>. Mirrors the
/// full configurable surface of <see cref="Abstractions.SanitizationSettings"/> (excluding
/// <c>CorrelationId</c>, which is per-call/per-request rather than a global setting) so every
/// sanitization concern — log injection, file path, sensitive data, regex injection — can be tuned
/// via configuration without code changes.
/// </summary>
public sealed class SanitizationFeatureOptions : FeatureOptionsBase
{
    // --- Log Injection Sanitization Settings ---

    /// <summary>Whether log sanitization is enabled. Default: true.</summary>
    public bool EnableLogSanitization { get; set; } = true;

    /// <summary>Whether Unicode control characters should be sanitized. Default: true.</summary>
    public bool SanitizeUnicodeControlChars { get; set; } = true;

    /// <summary>Whether tab characters are preserved rather than sanitized. Default: false.</summary>
    public bool PreserveTabCharacters { get; set; } = false;

    /// <summary>The strategy used to handle control characters during log sanitization. Default: Remove.</summary>
    public SanitizationStrategy LogSanitizationStrategy { get; set; } = SanitizationStrategy.Remove;

    /// <summary>Maximum length for sanitized strings before truncation. Default: 10000.</summary>
    public int MaxSanitizedStringLength { get; set; } = 10000;

    // --- Failure / Security Event Logging ---

    /// <summary>Whether sanitization failures are logged. Default: false.</summary>
    public bool LogSanitizationFailures { get; set; } = false;

    /// <summary>The log level used when logging sanitization failures. Default: Warning.</summary>
    public LogLevel SanitizationFailureLogLevel { get; set; } = LogLevel.Warning;

    /// <summary>Whether file-path security events are logged. Default: false.</summary>
    public bool LogSecurityEvents { get; set; } = false;

    // --- Performance Monitoring ---

    /// <summary>Whether performance monitoring is enabled. Default: false.</summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;

    /// <summary>The threshold, in milliseconds, above which an operation is considered slow. Default: 1.0.</summary>
    public double SlowOperationThresholdMs { get; set; } = 1.0;

    // --- File Path Sanitization Settings ---

    /// <summary>Whether file-path sanitization is enabled. Default: true.</summary>
    public bool EnableFilePathSanitization { get; set; } = true;

    /// <summary>Whether parent-directory traversal patterns are checked and removed. Default: true.</summary>
    public bool EnableParentDirectoryTraversalCheck { get; set; } = true;

    /// <summary>Whether strict allowlist validation is used for file paths. Default: true.</summary>
    public bool UseStrictValidation { get; set; } = true;

    /// <summary>Whether Windows reserved device names (CON, PRN, AUX, etc.) are rejected. Default: true.</summary>
    public bool ValidateWindowsReservedNames { get; set; } = true;

    /// <summary>Whether Unicode characters are allowed in file paths. Default: true.</summary>
    public bool AllowUnicodeCharacters { get; set; } = true;

    /// <summary>Maximum allowed length for an individual path segment. Default: 255.</summary>
    public int MaxPathSegmentLength { get; set; } = 255;

    /// <summary>Base directories file paths must resolve within. Empty means no restriction. Default: empty.</summary>
    public string[] AllowedBaseDirectories { get; set; } = Array.Empty<string>();

    /// <summary>File extensions a file path must end with (case-insensitive). Empty means no restriction. Default: empty.</summary>
    public string[] AllowedFileExtensions { get; set; } = Array.Empty<string>();

    // --- Sensitive Data Sanitization Settings ---

    /// <summary>Whether sensitive-data detection and masking is enabled. Default: true.</summary>
    public bool EnableSensitiveDataDetection { get; set; } = true;

    /// <summary>The character used to mask detected sensitive data. Default: '*'.</summary>
    public char SensitiveDataMaskCharacter { get; set; } = '*';

    /// <summary>The detection strictness level for sensitive data. Default: Medium.</summary>
    public SensitiveDataDetectionStrictness SensitiveDataDetectionStrictness { get; set; } = SensitiveDataDetectionStrictness.Medium;

    // --- Regex Injection Sanitization Settings ---

    /// <summary>Whether regex-pattern safety validation is enabled. Default: true.</summary>
    public bool EnableRegexSanitization { get; set; } = true;

    /// <summary>Maximum time allowed to validate a single regex pattern. Default: 5 seconds.</summary>
    public TimeSpan MaxRegexValidationTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Maximum allowed length for a regex pattern. Default: 1000.</summary>
    public int MaxRegexPatternLength { get; set; } = 1000;

    /// <summary>Maximum allowed complexity score for a regex pattern. Default: 100.</summary>
    public int MaxRegexComplexityScore { get; set; } = 100;

    /// <summary>Whether Unicode category classes (e.g. <c>\p{L}</c>) are allowed in patterns. Default: true.</summary>
    public bool AllowUnicodeCategories { get; set; } = true;

    /// <summary>Whether quantifiers (<c>*</c>, <c>+</c>, <c>?</c>, <c>{n,m}</c>) are allowed in patterns. Default: true.</summary>
    public bool AllowQuantifiers { get; set; } = true;

    /// <summary>Whether nested quantifiers (a common catastrophic-backtracking source) are allowed. Default: false.</summary>
    public bool AllowNestedQuantifiers { get; set; } = false;

    /// <summary>Whether backreferences are allowed in patterns. Default: true.</summary>
    public bool AllowBackreferences { get; set; } = true;

    /// <summary>Whether lookaround assertions are allowed in patterns. Default: true.</summary>
    public bool AllowLookarounds { get; set; } = true;
}

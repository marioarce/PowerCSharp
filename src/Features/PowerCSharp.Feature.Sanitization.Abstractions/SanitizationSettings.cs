using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Configuration settings for the sanitization engine. Controls the behavior of every
/// sanitization operation (log injection, file path, sensitive data, regex injection).
/// </summary>
public sealed class SanitizationSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether log sanitization is enabled.
    /// When disabled, log messages will not be sanitized for injection attacks.
    /// Default: true
    /// </summary>
    public bool EnableLogSanitization { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether sanitization failures should be logged.
    /// Useful for monitoring and debugging sanitization issues.
    /// Default: false (to prevent log spam and potential recursive issues)
    /// </summary>
    public bool LogSanitizationFailures { get; set; } = false;

    /// <summary>
    /// Gets or sets the log level to use when logging sanitization failures.
    /// Only used when <see cref="LogSanitizationFailures"/> is true.
    /// Default: Warning
    /// </summary>
    public LogLevel SanitizationFailureLogLevel { get; set; } = LogLevel.Warning;

    /// <summary>
    /// Gets or sets the maximum length for sanitized strings.
    /// Strings longer than this will be truncated to prevent memory issues.
    /// Default: 10000 characters
    /// </summary>
    public int MaxSanitizedStringLength { get; set; } = 10000;

    /// <summary>
    /// Gets or sets a value indicating whether Unicode control characters should be sanitized.
    /// Includes characters in ranges 0x00-0x1F, 0x7F-0x9F.
    /// Default: true
    /// </summary>
    public bool SanitizeUnicodeControlChars { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether file path sanitization is enabled.
    /// When enabled, file paths will be sanitized to prevent directory traversal attacks (CWE-73/CWE-22).
    /// Default: true
    /// </summary>
    public bool EnableFilePathSanitization { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether parent directory traversal patterns should be checked and removed.
    /// When enabled, patterns like "../" and "..\" will be removed from file paths.
    /// This can be disabled in production environments where path traversal is handled by other security measures.
    /// Default: true
    /// </summary>
    public bool EnableParentDirectoryTraversalCheck { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable performance monitoring for sanitization operations.
    /// When enabled, processing times are tracked and can be monitored.
    /// Default: false (minimal overhead)
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;

    /// <summary>
    /// Gets or sets the threshold in milliseconds for considering sanitization operations as slow.
    /// Operations taking longer than this threshold may be logged for performance analysis.
    /// Only used when <see cref="EnablePerformanceMonitoring"/> is true.
    /// Default: 1ms
    /// </summary>
    public double SlowOperationThresholdMs { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the allowed base directories for file path validation.
    /// When specified, file paths must resolve to within these directories.
    /// Empty array means no base directory validation is performed.
    /// Default: empty array (no base directory validation)
    /// </summary>
    public string[] AllowedBaseDirectories { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the allowed file extensions for file path validation.
    /// When specified, file paths must end with one of these extensions (case-insensitive).
    /// Empty array means no extension validation is performed.
    /// Default: empty array (no extension validation)
    /// </summary>
    public string[] AllowedFileExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether to use strict validation mode for file paths.
    /// When true, all validation rules are strictly enforced and violations result in rejection.
    /// When false, some non-critical violations may be sanitized instead of rejected.
    /// Default: true
    /// </summary>
    public bool UseStrictValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum allowed length for individual file path segments.
    /// Path segments longer than this will be rejected to prevent potential issues.
    /// Default: 255 characters (common max component length on Windows)
    /// </summary>
    public int MaxPathSegmentLength { get; set; } = 255;

    /// <summary>
    /// Gets or sets a value indicating whether to log security events.
    /// When enabled, blocked path traversal attempts and other security violations are logged.
    /// Default: false (to prevent log spam and potential information disclosure)
    /// </summary>
    public bool LogSecurityEvents { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to validate against Windows reserved device names.
    /// When true, paths containing reserved names like CON, PRN, AUX, etc. will be rejected.
    /// Default: true
    /// </summary>
    public bool ValidateWindowsReservedNames { get; set; } = true;

    /// <summary>
    /// Gets or sets the sanitization strategy for handling control characters.
    /// Determines how control characters are processed during log sanitization.
    /// Default: Remove (aggressive sanitization for clean logs)
    /// </summary>
    public SanitizationStrategy LogSanitizationStrategy { get; set; } = SanitizationStrategy.Remove;

    /// <summary>
    /// Gets or sets a value indicating whether to preserve tab characters.
    /// Tab characters (\t) are control characters but are often safe in logging contexts.
    /// Default: false (remove tabs for cleaner log output)
    /// </summary>
    public bool PreserveTabCharacters { get; set; } = false;

    /// <summary>
    /// Gets or sets the correlation ID for security event logging.
    /// When set, security events will include this correlation ID for traceability.
    /// Default: null (no correlation ID)
    /// </summary>
    public string? CorrelationId { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether to allow Unicode characters in file paths.
    /// When false, only ASCII characters are allowed to prevent Unicode-based attacks.
    /// Default: true (allow Unicode for internationalization)
    /// </summary>
    public bool AllowUnicodeCharacters { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether sensitive data detection is enabled.
    /// When enabled, sensitive data patterns will be detected and masked.
    /// Default: true
    /// </summary>
    public bool EnableSensitiveDataDetection { get; set; } = true;

    /// <summary>
    /// Gets or sets the character to use for masking detected sensitive data.
    /// This character will replace detected sensitive patterns.
    /// Default: '*' (asterisk)
    /// </summary>
    public char SensitiveDataMaskCharacter { get; set; } = '*';

    /// <summary>
    /// Gets or sets the detection strictness level for sensitive data.
    /// Higher values result in more aggressive detection (more false positives).
    /// Lower values result in more conservative detection (more false negatives).
    /// Default: Medium
    /// </summary>
    public SensitiveDataDetectionStrictness SensitiveDataDetectionStrictness { get; set; } = SensitiveDataDetectionStrictness.Medium;

    // --- Regex Injection Sanitization Settings ---

    /// <summary>
    /// Gets or sets a value indicating whether regex injection sanitization is enabled.
    /// When enabled, regex patterns will be validated for safety before use.
    /// Default: true
    /// </summary>
    public bool EnableRegexSanitization { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum timeout for regex pattern validation.
    /// Patterns that take longer than this to validate will be rejected.
    /// Default: 5 seconds
    /// </summary>
    public TimeSpan MaxRegexValidationTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the maximum allowed length for regex patterns.
    /// Patterns longer than this will be rejected to prevent complex attacks.
    /// Default: 1000 characters
    /// </summary>
    public int MaxRegexPatternLength { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum complexity score allowed for regex patterns.
    /// Higher scores indicate more complex patterns that could cause performance issues.
    /// Default: 100
    /// </summary>
    public int MaxRegexComplexityScore { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether to allow Unicode character categories in regex patterns.
    /// When false, Unicode categories like \p{L} will be rejected.
    /// Default: true (allow Unicode for internationalization)
    /// </summary>
    public bool AllowUnicodeCategories { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to allow quantifiers in regex patterns.
    /// When false, quantifiers like *, +, ?, {n,m} will be rejected.
    /// Default: true (allow quantifiers for functionality)
    /// </summary>
    public bool AllowQuantifiers { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to allow nested quantifiers in regex patterns.
    /// Nested quantifiers are a common source of catastrophic backtracking.
    /// Default: false (disallow nested quantifiers for safety)
    /// </summary>
    public bool AllowNestedQuantifiers { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to allow backreferences in regex patterns.
    /// Backreferences can lead to complex matching behavior.
    /// Default: true (allow backreferences for advanced patterns)
    /// </summary>
    public bool AllowBackreferences { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to allow lookaround assertions in regex patterns.
    /// Lookarounds can increase pattern complexity significantly.
    /// Default: true (allow lookarounds for advanced patterns)
    /// </summary>
    public bool AllowLookarounds { get; set; } = true;
}

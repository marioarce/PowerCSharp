using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// High-performance sanitization engine covering log injection (CWE-117), file-path traversal
/// (CWE-22), sensitive-data exposure (CWE-200), and regex-injection/ReDoS (CWE-400/CWE-730).
/// Designed for hot-path scenarios: minimal allocations and fail-safe behavior (sanitization
/// methods never throw; validation methods signal rejection via their result type instead).
/// </summary>
/// <remarks>
/// This partial class is split by concern across several files for maintainability:
/// <list type="bullet">
/// <item><description><c>SanitizationEngine.cs</c> (this file) — shared state, configuration wiring, performance tracking, and control-character classification shared by more than one concern.</description></item>
/// <item><description><c>SanitizationEngine.LogInjection.cs</c> — <see cref="SanitizeForLogInjection"/> and its encoding strategies.</description></item>
/// <item><description><c>SanitizationEngine.FilePath.cs</c> — <see cref="SanitizeForFilePath"/>, <see cref="SanitizeCorrelationIdForPath"/>, and allowlist path validation.</description></item>
/// <item><description><c>SanitizationEngine.SensitiveData.cs</c> — <see cref="SanitizeForSensitiveData"/> and masking heuristics.</description></item>
/// <item><description><c>SanitizationEngine.RegexInjection.cs</c> — <see cref="SanitizeForRegexInjection"/> and ReDoS complexity scoring.</description></item>
/// </list>
/// </remarks>
public static partial class SanitizationEngine
{
    // Constants for control character ranges, shared by log-injection sanitization and
    // file-path character validation.
    private const char AsciiControlCharStart = '\x00';
    private const char AsciiControlCharEnd = '\x1F';
    private const char ExtendedControlCharStart = '\x7F';
    private const char ExtendedControlCharEnd = '\x9F';

    // Configuration provider delegate - set by the hosting layer during startup.
    private static Func<SanitizationSettings>? _settingsProvider;

    // Security event logging delegate - set by the hosting layer during startup.
    private static Action<string>? _securityEventLogger;

    // Performance tracking fields.
    private static long _totalOperations;
    private static long _totalProcessingTimeMs;
    private static long _slowOperations;
    private static readonly object _performanceLock = new();

    // Default settings fallback (used when no provider has been configured).
    private static readonly SanitizationSettings _defaultSettings = new();

    /// <summary>
    /// Sets the configuration provider for sanitization settings.
    /// Call this once during application startup so every sanitization call reflects the
    /// host's current configuration without each call site needing to pass settings explicitly.
    /// </summary>
    /// <param name="provider">The function that provides current sanitization settings.</param>
    /// <param name="securityLogger">Optional action for logging security events.</param>
    public static void SetConfigurationProvider(
        Func<SanitizationSettings> provider,
        Action<string>? securityLogger = null)
    {
        _settingsProvider = provider
            ?? throw new ArgumentNullException(nameof(provider));

        _securityEventLogger = securityLogger;
    }

    /// <summary>
    /// Sets the security event logger used for CWE-22/CWE-73-style path validation violations.
    /// </summary>
    /// <param name="securityLogger">The action invoked for each logged security event.</param>
    public static void SetSecurityEventLogger(Action<string> securityLogger)
    {
        _securityEventLogger = securityLogger
            ?? throw new ArgumentNullException(nameof(securityLogger));
    }

    /// <summary>
    /// Gets performance statistics for the sanitization engine. Useful for monitoring and
    /// optimization when <see cref="SanitizationSettings.EnablePerformanceMonitoring"/> is enabled.
    /// </summary>
    /// <returns>A dictionary containing performance metrics.</returns>
    public static Dictionary<string, object> GetPerformanceStats()
    {
        lock (_performanceLock)
        {
            var averageProcessingTimeMs = _totalOperations > 0
                ? (double)_totalProcessingTimeMs / _totalOperations
                : 0.0;

            return new Dictionary<string, object>
            {
                ["EngineInitialized"] = true,
                ["TotalOperations"] = _totalOperations,
                ["AverageProcessingTimeMs"] = Math.Round(averageProcessingTimeMs, 3),
                ["SlowOperations"] = _slowOperations,
                ["SlowOperationPercentage"] = _totalOperations > 0
                    ? Math.Round((double)_slowOperations / _totalOperations * 100, 2)
                    : 0.0,
                ["SupportedSanitizationTypes"] = new[]
                {
                    SanitizationType.LogInjection.ToString(),
                    SanitizationType.FilePath.ToString(),
                    SanitizationType.SensitiveData.ToString(),
                    SanitizationType.RegexInjection.ToString()
                },
                ["SupportedSanitizationStrategies"] = new[]
                {
                    SanitizationStrategy.Remove.ToString(),
                    SanitizationStrategy.ReplaceWithSpace.ToString(),
                    SanitizationStrategy.HtmlEncode.ToString(),
                    SanitizationStrategy.UrlEncode.ToString(),
                    SanitizationStrategy.JsonEncode.ToString()
                }
            };
        }
    }

    /// <summary>
    /// Gets the current sanitization settings from the configuration provider, or the built-in
    /// defaults when no provider has been configured via <see cref="SetConfigurationProvider"/>.
    /// </summary>
    /// <returns>Current sanitization settings.</returns>
    private static SanitizationSettings GetCurrentSettings()
    {
        return _settingsProvider?.Invoke()
            ?? _defaultSettings;
    }

    /// <summary>
    /// Determines if a character is a control character based on settings. Shared by log-injection
    /// sanitization and file-path allowlist character validation.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>True if the character should be treated as a control character, false otherwise.</returns>
    private static bool IsControlCharacter(char c, SanitizationSettings settings)
    {
        if (c is >= AsciiControlCharStart and <= AsciiControlCharEnd)
        {
            return true;
        }

        if (settings.SanitizeUnicodeControlChars &&
            c is >= ExtendedControlCharStart and <= ExtendedControlCharEnd)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// <c>string.Contains(string, StringComparison)</c> is not available on <c>netstandard2.0</c>
    /// (added in .NET Standard 2.1). This helper provides the same behavior via
    /// <see cref="string.IndexOf(string, StringComparison)"/>, which is available on both target
    /// frameworks, so call sites shared across TFMs don't need <c>#if</c> guards.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="substring">The substring to search for.</param>
    /// <param name="comparison">The string comparison to use.</param>
    /// <returns>True if <paramref name="substring"/> occurs within <paramref name="value"/>.</returns>
    private static bool ContainsOrdinal(string value, string substring, StringComparison comparison)
        => value.IndexOf(substring, comparison) >= 0;

    /// <summary>
    /// Updates performance metrics for monitoring. No-op unless
    /// <see cref="SanitizationSettings.EnablePerformanceMonitoring"/> is enabled.
    /// </summary>
    /// <param name="processingTime">The time taken for the operation.</param>
    /// <param name="settings">The sanitization settings.</param>
    private static void UpdatePerformanceMetrics(TimeSpan processingTime, SanitizationSettings settings)
    {
        if (!settings.EnablePerformanceMonitoring)
        {
            return;
        }

        lock (_performanceLock)
        {
            _totalOperations++;
            _totalProcessingTimeMs += (long)processingTime.TotalMilliseconds;

            if (processingTime.TotalMilliseconds > settings.SlowOperationThresholdMs)
            {
                _slowOperations++;
            }
        }
    }

    /// <summary>
    /// Logs sanitization failures for security monitoring. No-op unless
    /// <see cref="SanitizationSettings.LogSanitizationFailures"/> is enabled and a security logger
    /// has been configured via <see cref="SetConfigurationProvider"/> or <see cref="SetSecurityEventLogger"/>.
    /// </summary>
    /// <param name="input">The input that failed sanitization.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <param name="eventType">The type of failure event.</param>
    private static void LogSanitizationFailure(string? input, SanitizationSettings settings, string eventType)
    {
        if (!settings.LogSanitizationFailures || _securityEventLogger == null)
        {
            return;
        }

        var correlationId = string.IsNullOrEmpty(settings.CorrelationId) ? "" : $" [CorrelationId: {settings.CorrelationId}]";
        var inputPreview = string.IsNullOrEmpty(input) ? "<null>" : input!.Length > 100 ? $"{input.Substring(0, 100)}..." : input;

        var message = $"CWE-117 Sanitization Failure: {eventType}{correlationId} - Input: '{inputPreview}'";
        _securityEventLogger(message);
    }

    /// <summary>
    /// Logs security events for CWE-22 file-path validation violations. No-op unless
    /// <see cref="SanitizationSettings.LogSecurityEvents"/> is enabled and a security logger has
    /// been configured.
    /// </summary>
    /// <param name="eventType">The type of security event.</param>
    /// <param name="input">The input that triggered the event.</param>
    /// <param name="settings">The sanitization settings.</param>
    private static void LogSecurityEvent(string eventType, string input, SanitizationSettings settings)
    {
        if (!settings.LogSecurityEvents || _securityEventLogger == null)
        {
            return;
        }

        // Sanitize the input first so the security log itself cannot be used for log injection (CWE-117).
        var sanitizedInput = SanitizeForLogInjection(input, settings).SanitizedValue;
        var inputPreview = sanitizedInput.Length > 100 ? $"{sanitizedInput.Substring(0, 100)}..." : sanitizedInput;

        var correlationId = string.IsNullOrEmpty(settings.CorrelationId) ? "" : $" [CorrelationId: {settings.CorrelationId}]";
        var message = $"CWE-22 Security Violation: {eventType}{correlationId} - Input: '{inputPreview}'";
        _securityEventLogger(message);
    }
}

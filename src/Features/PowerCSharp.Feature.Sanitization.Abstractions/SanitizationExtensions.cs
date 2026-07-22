namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// String extension methods wrapping the static <see cref="SanitizationEngine"/>. These are the
/// primary call-site API: no dependency injection or feature-flag wiring is required to use them —
/// reference this package and call <c>input.SanitizeForLog()</c> from anywhere, including hot-path
/// logging call sites.
/// </summary>
public static class SanitizationExtensions
{
    /// <summary>
    /// Sanitizes the string for safe logging by removing or encoding control characters.
    /// Prevents log injection (CWE-117) from carriage returns, line feeds, tabs, null bytes, and
    /// other control characters.
    /// </summary>
    /// <param name="input">The string to sanitize for logging. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>The sanitized string, safe for logging. Never null.</returns>
    /// <remarks>Fail-safe: never throws. If sanitization fails internally, the original string is returned.</remarks>
    public static string SanitizeForLog(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForLogInjection(input, settings).SanitizedValue;

    /// <summary>
    /// Sanitizes the string for safe logging and returns detailed sanitization results (whether it
    /// was modified, and how long the operation took).
    /// </summary>
    /// <param name="input">The string to sanitize for logging. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the sanitized string and operation details.</returns>
    public static SanitizationResult SanitizeForLogWithDetails(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForLogInjection(input, settings);

    /// <summary>
    /// Sanitizes the string for safe file-path usage, preventing directory traversal (CWE-22).
    /// </summary>
    /// <param name="input">The file-path string to sanitize. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>The sanitized file-path string. Never null.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the input is rejected under strict validation.</exception>
    /// <remarks>
    /// Sanitization failures never throw. In strict validation mode, however, a rejected path
    /// throws <see cref="InvalidOperationException"/> deliberately — silently returning an empty
    /// string for a rejected path would be a worse failure mode for callers that build a file-system
    /// path from the result. Use <see cref="SanitizeForFilePathWithDetails"/> to handle rejections
    /// programmatically instead.
    /// </remarks>
    public static string SanitizeForFilePath(this string? input, SanitizationSettings? settings = null)
    {
        var result = SanitizationEngine.SanitizeForFilePath(input, settings);

        if (result.IsRejected)
        {
            throw new InvalidOperationException(
                $"File path '{input}' was rejected by sanitization validation. " +
                "Use SanitizeForFilePathWithDetails() to handle rejections programmatically.");
        }

        return result.SanitizedValue;
    }

    /// <summary>
    /// Sanitizes the string for safe file-path usage and returns detailed sanitization results,
    /// including whether the input was rejected under strict validation.
    /// </summary>
    /// <param name="input">The file-path string to sanitize. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated file path and operation details.</returns>
    public static SanitizationResult SanitizeForFilePathWithDetails(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForFilePath(input, settings);

    /// <summary>
    /// Sanitizes the string for sensitive data by masking detected tokens, secrets, credentials,
    /// URLs, and file-system paths (CWE-200).
    /// </summary>
    /// <param name="input">The string to check for sensitive data. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>The string with sensitive data masked. Never null.</returns>
    public static string SanitizeForSensitiveData(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForSensitiveData(input, settings).SanitizedValue;

    /// <summary>
    /// Sanitizes the string for sensitive data and returns detailed sanitization results.
    /// </summary>
    /// <param name="input">The string to check for sensitive data. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SensitiveDataResult"/> containing the masked string and operation details.</returns>
    public static SensitiveDataResult SanitizeForSensitiveDataWithDetails(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForSensitiveData(input, settings);

    /// <summary>
    /// Validates the string as a regex pattern safe from ReDoS (CWE-400/CWE-730), returning the
    /// pattern unchanged when safe.
    /// </summary>
    /// <param name="input">The regex pattern to validate. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>The pattern, unchanged, if it passed validation. Never null.</returns>
    public static string SanitizeForRegexInjection(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForRegexInjection(input, settings).SanitizedValue;

    /// <summary>
    /// Validates the string as a regex pattern and returns detailed sanitization results, including
    /// whether the pattern was rejected as unsafe.
    /// </summary>
    /// <param name="input">The regex pattern to validate. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated pattern and operation details.</returns>
    public static SanitizationResult SanitizeForRegexInjectionWithDetails(this string? input, SanitizationSettings? settings = null)
        => SanitizationEngine.SanitizeForRegexInjection(input, settings);

    /// <summary>
    /// Masks the source string with the given mask character, keeping roughly a quarter of the
    /// string visible on each side (1/<c>visibility</c> total length as a fixed visibility window).
    /// </summary>
    /// <param name="value">The value to mask.</param>
    /// <param name="mask">The character to use for masking.</param>
    /// <returns>The masked value.</returns>
    public static string Mask(this string value, char mask)
    {
        const int visibility = 4;

        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var visibleCharLength = value.Length / visibility;
        return Mask(value, visibleCharLength, mask);
    }

    /// <summary>
    /// Masks the source string with the given mask character, keeping <paramref name="visibleCharLength"/>
    /// characters visible at the start and end of the value.
    /// </summary>
    /// <param name="value">The value to mask.</param>
    /// <param name="visibleCharLength">The number of characters to leave visible at the start and end.</param>
    /// <param name="mask">The character to use for masking.</param>
    /// <returns>The masked value.</returns>
    public static string Mask(this string value, int visibleCharLength, char mask)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length <= visibleCharLength * 2)
        {
            return new string(mask, value.Length);
        }

        var prefix = value.Substring(0, visibleCharLength);
        var suffix = value.Substring(value.Length - visibleCharLength);
        var maskedMiddle = new string(mask, value.Length - (visibleCharLength * 2));

        return $"{prefix}{maskedMiddle}{suffix}";
    }
}

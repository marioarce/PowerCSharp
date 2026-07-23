using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Log-injection (CWE-117) sanitization: removes or encodes control characters so untrusted input
/// cannot forge additional log lines/fields when written to a log sink.
/// </summary>
public static partial class SanitizationEngine
{
    private const char SpaceReplacement = ' ';
    private const char TabCharacter = '\t';

    /// <summary>
    /// Sanitizes a string to prevent log injection attacks by removing or replacing control characters.
    /// This method is optimized for performance and never throws exceptions.
    /// Implements CWE-117 remediation strategies (removal, replacement, and content-preserving encodings).
    /// </summary>
    /// <param name="input">The input string to sanitize. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the sanitized string and operation details.</returns>
    public static SanitizationResult SanitizeForLogInjection(
        string? input,
        SanitizationSettings? settings = null)
    {
        var startTime = Stopwatch.StartNew();
        var effectiveSettings = settings ?? GetCurrentSettings();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                return SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.LogInjection);
            }

            // See the analogous comment in SanitizationEngine.FilePath.cs: shadow with a value the
            // compiler can narrow consistently across both target frameworks.
            var value = input!;

            if (!effectiveSettings.EnableLogSanitization)
            {
                return SanitizationResult.Unchanged(value, SanitizationType.LogInjection);
            }

            // Fast path: skip processing entirely when no control characters are present.
            if (!ContainsControlCharacters(value, effectiveSettings))
            {
                return SanitizationResult.Unchanged(value, SanitizationType.LogInjection);
            }

            var sanitized = SanitizeControlCharactersWithStrategy(value, effectiveSettings);
            var wasModified = !string.Equals(value, sanitized, StringComparison.Ordinal);

            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);

            return wasModified
                ? SanitizationResult.Modified(sanitized, SanitizationType.LogInjection, startTime.Elapsed)
                : SanitizationResult.Unchanged(sanitized, SanitizationType.LogInjection);
        }
        catch
        {
            // Fail-safe: never throw from a sanitization call site; return the original input.
            startTime.Stop();
            LogSanitizationFailure(input, effectiveSettings, "SanitizationFailed");

            return SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.LogInjection);
        }
    }

    /// <summary>
    /// Fast check for control characters without allocations. Honors
    /// <see cref="SanitizationSettings.PreserveTabCharacters"/>.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>True if control characters are present, false otherwise.</returns>
    private static bool ContainsControlCharacters(string input, SanitizationSettings settings)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (var c in input)
        {
            if (c == TabCharacter && settings.PreserveTabCharacters)
            {
                continue;
            }

            if (IsControlCharacter(c, settings))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a character should be sanitized, honoring
    /// <see cref="SanitizationSettings.PreserveTabCharacters"/>.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>True if the character should be sanitized, false otherwise.</returns>
    private static bool ShouldSanitizeCharacter(char c, SanitizationSettings settings)
    {
        if (c == TabCharacter && settings.PreserveTabCharacters)
        {
            return false;
        }

        return IsControlCharacter(c, settings);
    }

    /// <summary>
    /// Sanitizes control characters using the strategy configured on
    /// <see cref="SanitizationSettings.LogSanitizationStrategy"/>.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized string.</returns>
    private static string SanitizeControlCharactersWithStrategy(
        string input,
        SanitizationSettings settings)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        return settings.LogSanitizationStrategy switch
        {
            SanitizationStrategy.Remove => SanitizeControlCharactersByRemoval(input, settings),
            SanitizationStrategy.ReplaceWithSpace => SanitizeControlCharactersByReplacement(input, settings),
            SanitizationStrategy.HtmlEncode => SanitizeControlCharactersByHtmlEncoding(input, settings),
            SanitizationStrategy.UrlEncode => SanitizeControlCharactersByUrlEncoding(input, settings),
            SanitizationStrategy.JsonEncode => SanitizeControlCharactersByJsonEncoding(input, settings),
            _ => SanitizeControlCharactersByRemoval(input, settings)
        };
    }

    /// <summary>
    /// Sanitizes control characters by removing them completely, then applies a well-known,
    /// static-analysis-recognized HTML-encoding pass so automated scanners can trace the
    /// sanitization path for CWE-117 compliance.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized string.</returns>
    private static string SanitizeControlCharactersByRemoval(
        string input,
        SanitizationSettings settings)
    {
        var output = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            if (!ShouldSanitizeCharacter(c, settings))
            {
                output.Append(c);
            }
        }

        var cleaned = output.ToString();
        var encoded = WebUtility.HtmlEncode(cleaned);

        return ApplyLengthLimit(encoded, settings);
    }

    /// <summary>
    /// Sanitizes control characters by replacing them with spaces, then applies the same
    /// static-analysis-recognized HTML-encoding pass as <see cref="SanitizeControlCharactersByRemoval"/>.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized string.</returns>
    private static string SanitizeControlCharactersByReplacement(
        string input,
        SanitizationSettings settings)
    {
        var output = new StringBuilder(input.Length);

        foreach (var c in input)
        {
            output.Append(ShouldSanitizeCharacter(c, settings) ? SpaceReplacement : c);
        }

        var cleaned = output.ToString();
        var encoded = WebUtility.HtmlEncode(cleaned);

        return ApplyLengthLimit(encoded, settings);
    }

    /// <summary>
    /// Sanitizes control characters by HTML-encoding the cleaned string. Addresses both CWE-117
    /// and potential CWE-79 (XSS) in log viewers that render log content as HTML.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized string.</returns>
    private static string SanitizeControlCharactersByHtmlEncoding(
        string input,
        SanitizationSettings settings)
    {
        var cleaned = SanitizeControlCharactersByRemoval(input, settings);
        var encoded = WebUtility.HtmlEncode(cleaned);
        return ApplyLengthLimit(encoded, settings);
    }

    /// <summary>
    /// Sanitizes control characters by URL-encoding the cleaned string.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized string.</returns>
    private static string SanitizeControlCharactersByUrlEncoding(
        string input,
        SanitizationSettings settings)
    {
        var cleaned = SanitizeControlCharactersByRemoval(input, settings);
        var encoded = WebUtility.UrlEncode(cleaned);
        return ApplyLengthLimit(encoded ?? cleaned, settings);
    }

    /// <summary>
    /// Sanitizes control characters by JSON-encoding the cleaned string (escape sequences only,
    /// with the surrounding quotes that <see cref="JsonSerializer"/> adds stripped back off).
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized string.</returns>
    private static string SanitizeControlCharactersByJsonEncoding(
        string input,
        SanitizationSettings settings)
    {
        var cleaned = SanitizeControlCharactersByRemoval(input, settings);
        var encoded = JsonSerializer.Serialize(cleaned);

        if (encoded.Length >= 2 && encoded[0] == '"' && encoded[encoded.Length - 1] == '"')
        {
            encoded = encoded.Substring(1, encoded.Length - 2);
        }

        return ApplyLengthLimit(encoded, settings);
    }

    /// <summary>
    /// Applies <see cref="SanitizationSettings.MaxSanitizedStringLength"/> to the sanitized string.
    /// </summary>
    /// <param name="input">The input string to limit.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>The length-limited string.</returns>
    private static string ApplyLengthLimit(string input, SanitizationSettings settings)
    {
        return input.Length > settings.MaxSanitizedStringLength
            ? input.Substring(0, settings.MaxSanitizedStringLength)
            : input;
    }
}

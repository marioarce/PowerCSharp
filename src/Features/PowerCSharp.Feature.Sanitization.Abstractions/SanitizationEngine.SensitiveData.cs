using System.Diagnostics;
using System.Text.RegularExpressions;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Sensitive-data (CWE-200) detection and masking: finds and redacts tokens, secrets, credentials,
/// URLs, and file-system paths that should not appear in logs or diagnostic output.
/// </summary>
public static partial class SanitizationEngine
{
#if NET8_0_OR_GREATER
    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9+/=]{20,}", RegexOptions.IgnoreCase)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"sk-[A-Za-z0-9]{20,}")]
    private static partial Regex StripeApiKeyRegex();

    [GeneratedRegex(@"api_key\s*=\s*[A-Za-z0-9+/=]{16,}")]
    private static partial Regex ApiKeyRegex();

    [GeneratedRegex(@"token\s*=\s*[A-Za-z0-9+/=]{16,}")]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"secret\s*=\s*[A-Za-z0-9+/=]{16,}")]
    private static partial Regex SecretRegex();

    [GeneratedRegex(@"password\s*=\s*[^\s]{6,}")]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"https?://[^\s,;\""\\]+")]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"[A-Za-z]:\\[^\s]+")]
    private static partial Regex WindowsPathRegex();

    [GeneratedRegex(@"\\\\[^\s]+")]
    private static partial Regex UncPathRegex();

    [GeneratedRegex(@"/(?:home|Users|var|etc|opt|srv|tmp|app)/\S+", RegexOptions.IgnoreCase)]
    private static partial Regex UnixPathRegex();

    [GeneratedRegex(@"~/[^\s]+")]
    private static partial Regex HomePathRegex();

    [GeneratedRegex(@"[A-Za-z0-9+/=]{16,}")]
    private static partial Regex LongAlphanumericRegex();

    [GeneratedRegex(@"(?:""?(?:password|pwd|pass|secret|token|key|apikey|api_key|auth|authorization|credential|creds|private|confidential)""?\s*[:=]\s*""?)([^"",}\s]+)""?", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveKeyRegex();

    [GeneratedRegex(@"(?:encryption|decrypt|cipher|hash|salt|iv|nonce)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EncryptionKeyRegex();

    [GeneratedRegex(@"(?:admin|root|user|login|session)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex AdminKeyRegex();

    [GeneratedRegex(@"(?:bearer|jwt|oauth|access|refresh)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex AuthTokenRegex();

    [GeneratedRegex(@"(?:ip|range|cidr|subnet|network)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex NetworkKeyRegex();
#else
    private static readonly Regex _bearerTokenRegex = new(@"Bearer\s+[A-Za-z0-9+/=]{20,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex BearerTokenRegex() => _bearerTokenRegex;

    private static readonly Regex _stripeApiKeyRegex = new(@"sk-[A-Za-z0-9]{20,}", RegexOptions.Compiled);
    private static Regex StripeApiKeyRegex() => _stripeApiKeyRegex;

    private static readonly Regex _apiKeyRegex = new(@"api_key\s*=\s*[A-Za-z0-9+/=]{16,}", RegexOptions.Compiled);
    private static Regex ApiKeyRegex() => _apiKeyRegex;

    private static readonly Regex _tokenRegex = new(@"token\s*=\s*[A-Za-z0-9+/=]{16,}", RegexOptions.Compiled);
    private static Regex TokenRegex() => _tokenRegex;

    private static readonly Regex _secretRegex = new(@"secret\s*=\s*[A-Za-z0-9+/=]{16,}", RegexOptions.Compiled);
    private static Regex SecretRegex() => _secretRegex;

    private static readonly Regex _passwordRegex = new(@"password\s*=\s*[^\s]{6,}", RegexOptions.Compiled);
    private static Regex PasswordRegex() => _passwordRegex;

    private static readonly Regex _urlRegex = new(@"https?://[^\s,;\""\\]+", RegexOptions.Compiled);
    private static Regex UrlRegex() => _urlRegex;

    private static readonly Regex _windowsPathRegex = new(@"[A-Za-z]:\\[^\s]+", RegexOptions.Compiled);
    private static Regex WindowsPathRegex() => _windowsPathRegex;

    private static readonly Regex _uncPathRegex = new(@"\\\\[^\s]+", RegexOptions.Compiled);
    private static Regex UncPathRegex() => _uncPathRegex;

    private static readonly Regex _unixPathRegex = new(@"/(?:home|Users|var|etc|opt|srv|tmp|app)/\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex UnixPathRegex() => _unixPathRegex;

    private static readonly Regex _homePathRegex = new(@"~/[^\s]+", RegexOptions.Compiled);
    private static Regex HomePathRegex() => _homePathRegex;

    private static readonly Regex _longAlphanumericRegex = new(@"[A-Za-z0-9+/=]{16,}", RegexOptions.Compiled);
    private static Regex LongAlphanumericRegex() => _longAlphanumericRegex;

    private static readonly Regex _sensitiveKeyRegex = new(@"(?:""?(?:password|pwd|pass|secret|token|key|apikey|api_key|auth|authorization|credential|creds|private|confidential)""?\s*[:=]\s*""?)([^"",}\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex SensitiveKeyRegex() => _sensitiveKeyRegex;

    private static readonly Regex _encryptionKeyRegex = new(@"(?:encryption|decrypt|cipher|hash|salt|iv|nonce)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex EncryptionKeyRegex() => _encryptionKeyRegex;

    private static readonly Regex _adminKeyRegex = new(@"(?:admin|root|user|login|session)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex AdminKeyRegex() => _adminKeyRegex;

    private static readonly Regex _authTokenRegex = new(@"(?:bearer|jwt|oauth|access|refresh)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex AuthTokenRegex() => _authTokenRegex;

    private static readonly Regex _networkKeyRegex = new(@"(?:ip|range|cidr|subnet|network)[\s:=]*([^,}\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex NetworkKeyRegex() => _networkKeyRegex;
#endif

    /// <summary>
    /// Detects and masks sensitive data in a string to prevent information disclosure.
    /// This method is optimized for performance and never throws exceptions.
    /// Implements CWE-200 remediation via pattern-based detection and masking.
    /// </summary>
    /// <param name="input">The input string to check for sensitive data. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SensitiveDataResult"/> containing the masked string and operation details.</returns>
    public static SensitiveDataResult SanitizeForSensitiveData(
        string? input,
        SanitizationSettings? settings = null)
    {
        var startTime = Stopwatch.StartNew();
        var effectiveSettings = settings ?? GetCurrentSettings();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                return SensitiveDataResult.Unchanged(input ?? string.Empty);
            }

            // See the analogous comment in SanitizationEngine.FilePath.cs: shadow with a value the
            // compiler can narrow consistently across both target frameworks.
            var value = input!;

            if (!effectiveSettings.EnableSensitiveDataDetection)
            {
                return SensitiveDataResult.Unchanged(value);
            }

            if (!ContainsSensitiveDataPatterns(value, effectiveSettings))
            {
                return SensitiveDataResult.Unchanged(value);
            }

            var masked = MaskSensitiveData(value, effectiveSettings);
            var wasModified = !string.Equals(value, masked, StringComparison.Ordinal);

            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);

            return wasModified
                ? SensitiveDataResult.Modified(masked, startTime.Elapsed)
                : SensitiveDataResult.Unchanged(masked);
        }
        catch
        {
            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
            return SensitiveDataResult.Unchanged(input ?? string.Empty);
        }
    }

    /// <summary>
    /// Fast check for sensitive-data patterns without allocations, so the (allocating) masking
    /// pass is only run when something actually needs masking.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>True if sensitive patterns are present, false otherwise.</returns>
    private static bool ContainsSensitiveDataPatterns(string input, SanitizationSettings settings)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (BearerTokenRegex().IsMatch(input) ||
            StripeApiKeyRegex().IsMatch(input) ||
            ApiKeyRegex().IsMatch(input) ||
            TokenRegex().IsMatch(input) ||
            SecretRegex().IsMatch(input) ||
            PasswordRegex().IsMatch(input))
        {
            return true;
        }

        if (SensitiveKeyRegex().IsMatch(input) ||
            EncryptionKeyRegex().IsMatch(input) ||
            AdminKeyRegex().IsMatch(input) ||
            AuthTokenRegex().IsMatch(input) ||
            NetworkKeyRegex().IsMatch(input))
        {
            return true;
        }

        if (UrlRegex().IsMatch(input))
        {
            return true;
        }

        if (WindowsPathRegex().IsMatch(input) ||
            UncPathRegex().IsMatch(input) ||
            UnixPathRegex().IsMatch(input) ||
            HomePathRegex().IsMatch(input))
        {
            return true;
        }

        if (settings.SensitiveDataDetectionStrictness >= SensitiveDataDetectionStrictness.Medium)
        {
            if (LongAlphanumericRegex().IsMatch(input))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Masks sensitive-data patterns in the input string using the configured mask character.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The masked string.</returns>
    private static string MaskSensitiveData(string input, SanitizationSettings settings)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        var maskChar = settings.SensitiveDataMaskCharacter;
        var result = input;

        result = MaskKeyBasedValues(result, maskChar);

        result = BearerTokenRegex().Replace(result, m => $"Bearer {m.Value.Substring(7).Mask(maskChar)}");
        result = StripeApiKeyRegex().Replace(result, m => $"sk-{m.Value.Substring(3).Mask(maskChar)}");
        result = ApiKeyRegex().Replace(result, m => $"api_key={ExtractValueAfterEquals(m.Value).Mask(maskChar)}");
        result = TokenRegex().Replace(result, m => $"token={ExtractValueAfterEquals(m.Value).Mask(maskChar)}");
        result = SecretRegex().Replace(result, m => $"secret={ExtractValueAfterEquals(m.Value).Mask(maskChar)}");
        result = PasswordRegex().Replace(result, m => $"password={ExtractValueAfterEquals(m.Value).Mask(maskChar)}");

        result = UrlRegex().Replace(result, m => MaskValueIntelligently(m.Value, maskChar));

        result = WindowsPathRegex().Replace(result, m => m.Value.Mask(maskChar));
        result = UncPathRegex().Replace(result, m => m.Value.Mask(maskChar));
        result = UnixPathRegex().Replace(result, m => m.Value.Mask(maskChar));
        result = HomePathRegex().Replace(result, m => m.Value.Mask(maskChar));

        if (settings.SensitiveDataDetectionStrictness >= SensitiveDataDetectionStrictness.Medium)
        {
            result = LongAlphanumericRegex().Replace(result, m => m.Value.Mask(maskChar));
        }

        return result;
    }

    /// <summary>
    /// Masks sensitive values found via key-based heuristics (JSON key-value pairs and similar
    /// <c>key: value</c>/<c>key=value</c> formats).
    /// </summary>
    /// <param name="input">The input string to process.</param>
    /// <param name="maskChar">The character to use for masking.</param>
    /// <returns>The masked string.</returns>
    private static string MaskKeyBasedValues(string input, char maskChar)
    {
        var result = input;

        result = SensitiveKeyRegex().Replace(result, m =>
        {
            // SensitiveKeyRegex has exactly one capturing group (the value); the key/separator
            // prefix is matched but intentionally not captured. Reconstruct it the same way the
            // four handlers below do, rather than indexing a non-existent Groups[2] (which is
            // always an empty, unsuccessful group and previously left the raw value unmasked).
            var key = m.Value.Substring(0, m.Value.IndexOf(m.Groups[1].Value, StringComparison.Ordinal));
            var maskedValue = MaskValueIntelligently(m.Groups[1].Value, maskChar);
            return $"{key}{maskedValue}";
        });

        result = EncryptionKeyRegex().Replace(result, m =>
        {
            var key = m.Value.Substring(0, m.Value.IndexOf(m.Groups[1].Value, StringComparison.Ordinal));
            var maskedValue = MaskValueIntelligently(m.Groups[1].Value, maskChar);
            return $"{key}{maskedValue}";
        });

        result = AdminKeyRegex().Replace(result, m =>
        {
            var key = m.Value.Substring(0, m.Value.IndexOf(m.Groups[1].Value, StringComparison.Ordinal));
            var maskedValue = MaskValueIntelligently(m.Groups[1].Value, maskChar);
            return $"{key}{maskedValue}";
        });

        result = AuthTokenRegex().Replace(result, m =>
        {
            var key = m.Value.Substring(0, m.Value.IndexOf(m.Groups[1].Value, StringComparison.Ordinal));
            var maskedValue = MaskValueIntelligently(m.Groups[1].Value, maskChar);
            return $"{key}{maskedValue}";
        });

        result = NetworkKeyRegex().Replace(result, m =>
        {
            var key = m.Value.Substring(0, m.Value.IndexOf(m.Groups[1].Value, StringComparison.Ordinal));
            var maskedValue = MaskValueIntelligently(m.Groups[1].Value, maskChar);
            return $"{key}{maskedValue}";
        });

        return result;
    }

    /// <summary>
    /// Masks the middle portion of a value (keeping the first and last quarter visible) so masked
    /// output still hints at shape/length without disclosing the sensitive content.
    /// </summary>
    /// <param name="value">The value to mask.</param>
    /// <param name="maskChar">The character to use for masking.</param>
    /// <returns>The masked value.</returns>
    private static string MaskValueIntelligently(string value, char maskChar)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var result = value.ToCharArray();
        var length = result.Length;
        var maskStart = length / 4;
        var maskEnd = length - (length / 4);

        for (var i = maskStart; i < maskEnd && i < length; i++)
        {
            result[i] = maskChar;
        }

        return new string(result);
    }

    /// <summary>
    /// Extracts the value after the first <c>=</c> character. Avoids the allocation overhead of
    /// <c>Split().Last()</c> and correctly handles values that themselves contain <c>=</c>
    /// (common in Base64 tokens and signed URLs).
    /// </summary>
    /// <param name="keyValuePair">The string containing a <c>key=value</c> pair.</param>
    /// <returns>The value after the first <c>=</c> character, or the original string if none is found.</returns>
    private static string ExtractValueAfterEquals(string keyValuePair)
    {
        var equalsIndex = keyValuePair.IndexOf('=');

        return equalsIndex >= 0 && equalsIndex < keyValuePair.Length - 1
            ? keyValuePair.Substring(equalsIndex + 1)
            : keyValuePair;
    }
}

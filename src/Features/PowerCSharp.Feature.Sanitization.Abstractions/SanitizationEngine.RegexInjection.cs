using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// Regex-injection / ReDoS (CWE-400/CWE-730) sanitization: validates that a regex pattern is safe
/// to compile and execute before it is used against untrusted input.
/// </summary>
public static partial class SanitizationEngine
{
    /// <summary>
    /// Internal validation outcome for <see cref="ValidateRegexPatternSafety"/>. A plain struct
    /// (not a record struct) to avoid any dependency on <c>init</c>-accessor support, which the
    /// <c>netstandard2.0</c> target of this package cannot rely on being present.
    /// </summary>
    private readonly struct RegexValidationResult
    {
        public RegexValidationResult(bool isValid, string reason)
        {
            IsValid = isValid;
            Reason = reason;
        }

        public bool IsValid { get; }

        public string Reason { get; }
    }

    /// <summary>
    /// Validates a regex pattern to prevent Regular Expression Denial of Service (ReDoS) attacks.
    /// Checks pattern length, compilation safety, complexity, and known catastrophic-backtracking
    /// shapes (nested quantifiers) before the pattern is ever used against untrusted input.
    /// </summary>
    /// <param name="pattern">The regex pattern to validate. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated pattern and operation details.</returns>
    public static SanitizationResult SanitizeForRegexInjection(
        string? pattern,
        SanitizationSettings? settings = null)
    {
        var startTime = Stopwatch.StartNew();
        var effectiveSettings = settings ?? GetCurrentSettings();

        try
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return SanitizationResult.Unchanged(pattern ?? string.Empty, SanitizationType.RegexInjection);
            }

            // See the analogous comment in SanitizationEngine.FilePath.cs: shadow with a value the
            // compiler can narrow consistently across both target frameworks.
            var value = pattern!;

            if (!effectiveSettings.EnableRegexSanitization)
            {
                return SanitizationResult.Unchanged(value, SanitizationType.RegexInjection);
            }

            if (value.Length > effectiveSettings.MaxRegexPatternLength)
            {
                startTime.Stop();
                UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
                LogSecurityEvent("Regex pattern length exceeded", $"Regex pattern rejected: length {value.Length} exceeds maximum {effectiveSettings.MaxRegexPatternLength}", effectiveSettings);
                return SanitizationResult.Rejected(SanitizationType.RegexInjection, startTime.Elapsed);
            }

            var validationResult = ValidateRegexPatternSafety(value, effectiveSettings);

            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);

            if (!validationResult.IsValid)
            {
                LogSecurityEvent("Regex pattern validation failed", $"Regex pattern rejected: {validationResult.Reason}", effectiveSettings);
                return SanitizationResult.Rejected(SanitizationType.RegexInjection, startTime.Elapsed);
            }

            return SanitizationResult.Unchanged(value, SanitizationType.RegexInjection);
        }
        catch
        {
            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
            LogSanitizationFailure(pattern, effectiveSettings, "RegexSanitizationFailed");
            return SanitizationResult.Rejected(SanitizationType.RegexInjection, startTime.Elapsed);
        }
    }

    /// <summary>
    /// Runs every configured safety check against a regex pattern in order, short-circuiting on
    /// the first violation.
    /// </summary>
    /// <param name="pattern">The regex pattern to validate.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>A validation result indicating whether the pattern is safe.</returns>
    private static RegexValidationResult ValidateRegexPatternSafety(string pattern, SanitizationSettings settings)
    {
        if (!ValidateRegexSyntaxWithTimeout(pattern, settings.MaxRegexValidationTimeout))
        {
            return new RegexValidationResult(false, "Pattern compilation timeout or syntax error");
        }

        var complexityScore = CalculateRegexComplexityScore(pattern);
        if (complexityScore > settings.MaxRegexComplexityScore)
        {
            return new RegexValidationResult(false, $"Complexity score {complexityScore} exceeds maximum {settings.MaxRegexComplexityScore}");
        }

        if (!settings.AllowUnicodeCategories && ContainsUnicodeCategories(pattern))
        {
            return new RegexValidationResult(false, "Unicode character categories are not allowed");
        }

        if (!settings.AllowQuantifiers && ContainsQuantifiers(pattern))
        {
            return new RegexValidationResult(false, "Quantifiers are not allowed");
        }

        if (!settings.AllowNestedQuantifiers && ContainsNestedQuantifiers(pattern))
        {
            return new RegexValidationResult(false, "Nested quantifiers detected (high ReDoS risk)");
        }

        if (!settings.AllowBackreferences && ContainsBackreferences(pattern))
        {
            return new RegexValidationResult(false, "Backreferences are not allowed");
        }

        if (!settings.AllowLookarounds && ContainsLookarounds(pattern))
        {
            return new RegexValidationResult(false, "Lookaround assertions are not allowed");
        }

        return new RegexValidationResult(true, "Pattern is safe");
    }

    /// <summary>
    /// Validates regex syntax with a timeout, so an attacker-supplied pattern cannot hang the
    /// validation step itself.
    /// </summary>
    /// <param name="pattern">The regex pattern to validate.</param>
    /// <param name="timeout">The maximum time to allow for compilation.</param>
    /// <returns>True if the pattern compiles successfully within the timeout.</returns>
    private static bool ValidateRegexSyntaxWithTimeout(string pattern, TimeSpan timeout)
    {
        try
        {
            var cts = new CancellationTokenSource(timeout);
            var task = Task.Run(() =>
            {
                _ = new Regex(pattern, RegexOptions.Compiled);
                return true;
            }, cts.Token);

            return task.Wait(timeout) && task.Result;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calculates a complexity score for a regex pattern based on nesting depth, quantifier count,
    /// group count, and character-class/escape usage. Higher scores indicate patterns more likely
    /// to cause catastrophic backtracking or excessive compilation cost.
    /// </summary>
    /// <param name="pattern">The regex pattern to analyze.</param>
    /// <returns>A complexity score (higher = more complex), capped at 1000.</returns>
    private static int CalculateRegexComplexityScore(string pattern)
    {
        var score = 0;
        var depth = 0;
        var quantifierCount = 0;
        var groupCount = 0;

        foreach (var c in pattern)
        {
            switch (c)
            {
                case '(':
                    depth++;
                    groupCount++;
                    score += depth * 2;
                    break;
                case ')':
                    depth--;
                    break;
                case '*':
                case '+':
                case '?':
                    quantifierCount++;
                    score += 5;
                    break;
                case '{':
                    score += 10;
                    break;
                case '[':
                    score += 3;
                    break;
                case '\\':
                    score += 2;
                    break;
            }
        }

        if (quantifierCount > 5)
        {
            score += quantifierCount * 3;
        }

        if (groupCount > 10)
        {
            score += groupCount * 2;
        }

        return Math.Min(score, 1000);
    }

    /// <summary>
    /// Checks if a pattern contains Unicode character categories (<c>\p{...}</c> / <c>\P{...}</c>).
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if Unicode categories are found.</returns>
    private static bool ContainsUnicodeCategories(string pattern)
    {
        return ContainsOrdinal(pattern, @"\p{", StringComparison.Ordinal) || ContainsOrdinal(pattern, @"\P{", StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if a pattern contains quantifiers (<c>*</c>, <c>+</c>, <c>?</c>, or <c>{n,m}</c>).
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if quantifiers are found.</returns>
    private static bool ContainsQuantifiers(string pattern)
    {
        foreach (var c in pattern)
        {
            if (c is '*' or '+' or '?')
            {
                return true;
            }
        }

        return pattern.IndexOf('{') >= 0 && pattern.IndexOf('}') >= 0;
    }

    /// <summary>
    /// Checks if a pattern contains nested quantifiers (e.g. <c>(a+)+</c>, <c>(a*)*</c>) — the
    /// classic shape behind catastrophic backtracking.
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if nested quantifiers are found.</returns>
    private static bool ContainsNestedQuantifiers(string pattern)
    {
        const string groupPattern = @"\([^)]*[*+?][^)]*\)[*+?]";

        try
        {
            return Regex.IsMatch(pattern, groupPattern);
        }
        catch
        {
            return ManualNestedQuantifierDetection(pattern);
        }
    }

    /// <summary>
    /// Manual fallback for nested-quantifier detection when regex matching itself fails.
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if nested quantifiers are found.</returns>
    private static bool ManualNestedQuantifierDetection(string pattern)
    {
        var inGroup = false;
        var groupHasQuantifier = false;
        var i = 0;

        while (i < pattern.Length)
        {
            var c = pattern[i];

            switch (c)
            {
                case '(':
                    inGroup = true;
                    groupHasQuantifier = false;
                    break;
                case ')':
                    if (inGroup && groupHasQuantifier)
                    {
                        if (i + 1 < pattern.Length && IsQuantifier(pattern[i + 1]))
                        {
                            return true;
                        }
                    }
                    inGroup = false;
                    groupHasQuantifier = false;
                    break;
                case '*':
                case '+':
                case '?':
                    if (inGroup)
                    {
                        groupHasQuantifier = true;
                    }
                    break;
                case '{':
                    if (inGroup)
                    {
                        groupHasQuantifier = true;
                        var braceEnd = pattern.IndexOf('}', i);
                        if (braceEnd != -1)
                        {
                            i = braceEnd;
                        }
                    }
                    break;
            }

            i++;
        }

        return false;
    }

    /// <summary>
    /// Determines if a character is a regex quantifier.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is a quantifier.</returns>
    private static bool IsQuantifier(char c)
    {
        return c is '*' or '+' or '?';
    }

    /// <summary>
    /// Checks if a pattern contains backreferences (<c>\1</c>, <c>\k&lt;name&gt;</c>).
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if backreferences are found.</returns>
    private static bool ContainsBackreferences(string pattern)
    {
        for (var i = 0; i < pattern.Length - 1; i++)
        {
            if (pattern[i] == '\\' && char.IsDigit(pattern[i + 1]))
            {
                return true;
            }
        }

        return ContainsOrdinal(pattern, @"\k<", StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if a pattern contains lookaround assertions (<c>(?=</c>, <c>(?!</c>, <c>(?&lt;=</c>, <c>(?&lt;!</c>).
    /// </summary>
    /// <param name="pattern">The regex pattern to check.</param>
    /// <returns>True if lookarounds are found.</returns>
    private static bool ContainsLookarounds(string pattern)
    {
        return ContainsOrdinal(pattern, "?=", StringComparison.Ordinal) ||
               ContainsOrdinal(pattern, "?!", StringComparison.Ordinal) ||
               ContainsOrdinal(pattern, "?<=", StringComparison.Ordinal) ||
               ContainsOrdinal(pattern, "?<!", StringComparison.Ordinal) ||
               ContainsOrdinal(pattern, "?<", StringComparison.Ordinal) ||
               ContainsOrdinal(pattern, "?>", StringComparison.Ordinal);
    }
}

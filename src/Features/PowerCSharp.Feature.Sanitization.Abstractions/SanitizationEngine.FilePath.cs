using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Abstractions;

/// <summary>
/// File-path (CWE-22) sanitization: an allowlist-validation mode (recommended) plus a legacy
/// pattern-stripping mode, both guarding against directory traversal and absolute/UNC path injection.
/// </summary>
public static partial class SanitizationEngine
{
    private const string DoubleBackslash = "\\\\";
    private const string SingleBackslash = "\\";
    private const string ForwardSlash = "/";
    private const string DoubleForwardSlash = "//";
    private const string ParentDirectoryReference = "..";
    private const string Colon = ":";

    // Windows reserved device names.
    private static readonly string[] _windowsReservedNames =
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    private static readonly char[] _pathSeparators = { '/', '\\' };

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"^[a-zA-Z]:")]
    private static partial Regex DriveLetterRegex();

    [GeneratedRegex(@"(?<=[/\\])\.|\.(?=[/\\])|^\./|^\.")]
    private static partial Regex CurrentDirectoryReferenceRegex();

    [GeneratedRegex(@"[/\\]{2,}")]
    private static partial Regex MultipleSlashesRegex();
#else
    private static readonly Regex _driveLetterRegex = new(@"^[a-zA-Z]:", RegexOptions.Compiled);
    private static Regex DriveLetterRegex() => _driveLetterRegex;

    private static readonly Regex _currentDirectoryReferenceRegex = new(@"(?<=[/\\])\.|\.(?=[/\\])|^\./|^\.", RegexOptions.Compiled);
    private static Regex CurrentDirectoryReferenceRegex() => _currentDirectoryReferenceRegex;

    private static readonly Regex _multipleSlashesRegex = new(@"[/\\]{2,}", RegexOptions.Compiled);
    private static Regex MultipleSlashesRegex() => _multipleSlashesRegex;
#endif

    /// <summary>
    /// Sanitizes a file path to prevent directory traversal attacks (CWE-22). Uses allowlist
    /// validation (recommended) when <see cref="SanitizationSettings.UseStrictValidation"/> is
    /// true, otherwise falls back to legacy pattern-stripping.
    /// </summary>
    /// <param name="input">The input file path to sanitize. Can be null.</param>
    /// <param name="settings">Optional sanitization settings. If null, the configured or default settings are used.</param>
    /// <returns>A <see cref="SanitizationResult"/> containing the validated file path and operation details.</returns>
    public static SanitizationResult SanitizeForFilePath(
        string? input,
        SanitizationSettings? settings = null)
    {
        var startTime = Stopwatch.StartNew();
        var effectiveSettings = settings ?? GetCurrentSettings();

        try
        {
            if (string.IsNullOrEmpty(input))
            {
                startTime.Stop();
                UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
                return SanitizationResult.Unchanged(input ?? string.Empty, SanitizationType.FilePath);
            }

            // Narrowed to non-null/non-empty from here; netstandard2.0's reference assembly for
            // string.IsNullOrEmpty doesn't carry the [NotNullWhen] annotation net8.0's does, so the
            // compiler can't narrow `input` itself across TFMs — shadow with a value it can.
            var value = input!;

            if (!effectiveSettings.EnableFilePathSanitization)
            {
                startTime.Stop();
                UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
                return SanitizationResult.Unchanged(value, SanitizationType.FilePath);
            }

            if (effectiveSettings.UseStrictValidation)
            {
                var isValid = ValidateFilePathAllowlist(value, effectiveSettings);
                startTime.Stop();
                UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);

                return isValid
                    ? SanitizationResult.Unchanged(value, SanitizationType.FilePath)
                    : SanitizationResult.Rejected(SanitizationType.FilePath, startTime.Elapsed);
            }

            // Legacy mode: fast path when no dangerous patterns are present.
            if (!ContainsDangerousPathPatterns(value, effectiveSettings))
            {
                startTime.Stop();
                UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
                return SanitizationResult.Unchanged(value, SanitizationType.FilePath);
            }

            var sanitized = SanitizeFilePathLegacy(value, effectiveSettings);
            var wasModified = !string.Equals(value, sanitized, StringComparison.Ordinal);

            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);

            return wasModified
                ? SanitizationResult.Modified(sanitized, SanitizationType.FilePath, startTime.Elapsed)
                : SanitizationResult.Unchanged(sanitized, SanitizationType.FilePath);
        }
        catch
        {
            // Fail-safe: reject the path entirely rather than risk returning something unsafe.
            startTime.Stop();
            UpdatePerformanceMetrics(startTime.Elapsed, effectiveSettings);
            return SanitizationResult.Rejected(SanitizationType.FilePath, startTime.Elapsed);
        }
    }

    /// <summary>
    /// Sanitizes a correlation ID to make it safe for use as (part of) a file name. Removes
    /// invalid file-name characters and enforces a reasonable maximum length.
    /// </summary>
    /// <param name="correlationId">The correlation ID to sanitize.</param>
    /// <returns>A sanitized correlation ID safe for file-name use.</returns>
    public static string SanitizeCorrelationIdForPath(string correlationId)
    {
        const int maxCorrelationIdLength = 50;

        if (string.IsNullOrEmpty(correlationId))
        {
            return string.Empty;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder(correlationId.Length);

        foreach (var c in correlationId)
        {
            sanitized.Append(Array.IndexOf(invalidChars, c) >= 0 ? '_' : c);
        }

        var result = sanitized.ToString();

        if (result.Length > maxCorrelationIdLength)
        {
            result = result.Substring(0, maxCorrelationIdLength);
        }

        // Trim trailing dots/spaces so the result is safe on file systems that reject them.
        return result.TrimEnd('.', ' ');
    }

    /// <summary>
    /// Fast check for dangerous file-path patterns without allocations, used by legacy
    /// (non-strict) mode. Mirrors every pattern <see cref="SanitizeFilePathLegacy"/> handles so the
    /// documented protections are actually applied.
    /// </summary>
    /// <param name="input">The file path to check.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>True if dangerous patterns are present, false otherwise.</returns>
    private static bool ContainsDangerousPathPatterns(string input, SanitizationSettings settings)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (settings.EnableParentDirectoryTraversalCheck &&
            ContainsOrdinal(input, ParentDirectoryReference, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsOrdinal(input, DoubleForwardSlash, StringComparison.OrdinalIgnoreCase) ||
            ContainsOrdinal(input, DoubleBackslash, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsOrdinal(input, Colon, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (input.StartsWith(ForwardSlash, StringComparison.Ordinal) || input.StartsWith(SingleBackslash, StringComparison.Ordinal))
        {
            return true;
        }

        var hasParentRef = ContainsOrdinal(input, "../", StringComparison.OrdinalIgnoreCase) ||
                            ContainsOrdinal(input, "..\\", StringComparison.OrdinalIgnoreCase);

        if (ContainsOrdinal(input, "./", StringComparison.OrdinalIgnoreCase) && !hasParentRef)
        {
            return true;
        }

        if (ContainsOrdinal(input, ".\\", StringComparison.OrdinalIgnoreCase) && !hasParentRef)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sanitizes dangerous patterns in the file path using string replacement (legacy mode).
    /// Prefer <see cref="SanitizationSettings.UseStrictValidation"/> allowlist validation for new code.
    /// </summary>
    /// <param name="input">The input file path to sanitize.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>The sanitized file path.</returns>
    private static string SanitizeFilePathLegacy(
        string input,
        SanitizationSettings settings)
    {
        var sanitized = input;

        if (settings.EnableParentDirectoryTraversalCheck)
        {
            // These literals contain no alphabetic characters, so an ordinal (case-sensitive)
            // replace is behaviorally identical to an ordinal-ignore-case one, and the 2-arg
            // Replace(string, string) overload is available on netstandard2.0 (unlike the
            // 3-arg StringComparison overload, which is netstandard2.1+).
            sanitized = sanitized.Replace(ParentDirectoryReference, string.Empty);
        }

        sanitized = sanitized.Replace(DoubleForwardSlash, ForwardSlash);
        sanitized = sanitized.Replace(DoubleBackslash, SingleBackslash);
        sanitized = DriveLetterRegex().Replace(sanitized, string.Empty);
        sanitized = sanitized.TrimStart('/', '\\');
        sanitized = CurrentDirectoryReferenceRegex().Replace(sanitized, string.Empty);
        sanitized = MultipleSlashesRegex().Replace(sanitized, ForwardSlash);

        return sanitized.Length > settings.MaxSanitizedStringLength
            ? sanitized.Substring(0, settings.MaxSanitizedStringLength)
            : sanitized;
    }

    /// <summary>
    /// Validates a file path using allowlist rules per CWE-22 recommendations: reject anything
    /// suspicious rather than trying to "clean" it into something safe.
    /// </summary>
    /// <param name="input">The input file path to validate.</param>
    /// <param name="settings">The sanitization settings to apply.</param>
    /// <returns>True if the path is valid, false otherwise.</returns>
    private static bool ValidateFilePathAllowlist(string input, SanitizationSettings settings)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (ContainsInvalidCharacters(input, settings))
        {
            LogSecurityEvent("Invalid characters detected", input, settings);
            return false;
        }

        if (settings.ValidateWindowsReservedNames && IsWindowsReservedName(input))
        {
            LogSecurityEvent("Windows reserved device name detected", input, settings);
            return false;
        }

        if (HasInvalidPathSegmentLengths(input, settings))
        {
            LogSecurityEvent("Invalid path segment length", input, settings);
            return false;
        }

        if (settings.AllowedFileExtensions.Length > 0 && !IsValidFileExtension(input, settings))
        {
            LogSecurityEvent("Invalid file extension", input, settings);
            return false;
        }

        if (settings.AllowedBaseDirectories.Length == 0 && ContainsDangerousTraversalPatterns(input))
        {
            LogSecurityEvent("Dangerous traversal pattern detected", input, settings);
            return false;
        }

        if (settings.AllowedBaseDirectories.Length > 0 && !IsWithinAllowedDirectories(input, settings))
        {
            LogSecurityEvent("Path outside allowed directories", input, settings);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks for directory-traversal patterns, including common URL-encoded variants
    /// (<c>%2e%2e%2f</c> and similar) that a naive string check would miss.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns>True if dangerous traversal patterns are present, false otherwise.</returns>
    private static bool ContainsDangerousTraversalPatterns(string input)
    {
        if (ContainsOrdinal(input, "../", StringComparison.OrdinalIgnoreCase) ||
            ContainsOrdinal(input, "..\\", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (ContainsOrdinal(input, "%2e%2e%2f", StringComparison.OrdinalIgnoreCase) ||
            ContainsOrdinal(input, "%2e%2e%5c", StringComparison.OrdinalIgnoreCase) ||
            ContainsOrdinal(input, "..%2f", StringComparison.OrdinalIgnoreCase) ||
            ContainsOrdinal(input, "..%5c", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (input.StartsWith(ForwardSlash, StringComparison.Ordinal) || input.StartsWith(SingleBackslash, StringComparison.Ordinal))
        {
            return true;
        }

        if (input.Length >= 2 && input[1] == ':' && char.IsLetter(input[0]))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the input contains invalid characters based on settings: control characters,
    /// disallowed Unicode (when <see cref="SanitizationSettings.AllowUnicodeCharacters"/> is false),
    /// and characters invalid in Windows file names.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>True if invalid characters are present, false otherwise.</returns>
    private static bool ContainsInvalidCharacters(string input, SanitizationSettings settings)
    {
        foreach (var c in input)
        {
            if (IsControlCharacter(c, settings))
            {
                return true;
            }

            if (!settings.AllowUnicodeCharacters && c > 127)
            {
                return true;
            }

            if (IsInvalidPathCharacter(c))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if a character is invalid in Windows file names.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is invalid, false otherwise.</returns>
    private static bool IsInvalidPathCharacter(char c)
    {
        return c is '<' or '>' or ':' or '"' or '|' or '?' or '*';
    }

    /// <summary>
    /// Checks if the path contains a Windows reserved device name (e.g. <c>CON</c>, <c>PRN</c>,
    /// <c>COM1</c>) in any path segment.
    /// </summary>
    /// <param name="input">The input path to check.</param>
    /// <returns>True if a reserved name is found, false otherwise.</returns>
    private static bool IsWindowsReservedName(string input)
    {
        var normalizedPath = input.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        var pathSegments = normalizedPath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in pathSegments)
        {
            var segmentName = Path.GetFileNameWithoutExtension(segment);

            if (string.IsNullOrEmpty(segmentName))
            {
                continue;
            }

            var upperSegmentName = segmentName.ToUpperInvariant();

            foreach (var reservedName in _windowsReservedNames)
            {
                if (upperSegmentName == reservedName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if any path segment exceeds <see cref="SanitizationSettings.MaxPathSegmentLength"/>.
    /// </summary>
    /// <param name="input">The input path to check.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>True if an invalid segment length is found, false otherwise.</returns>
    private static bool HasInvalidPathSegmentLengths(string input, SanitizationSettings settings)
    {
        var segments = input.Split(_pathSeparators, StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            if (segment.Length > settings.MaxPathSegmentLength)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates the file extension against <see cref="SanitizationSettings.AllowedFileExtensions"/>.
    /// </summary>
    /// <param name="input">The input path to check.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>True if the extension is allowed, false otherwise.</returns>
    private static bool IsValidFileExtension(string input, SanitizationSettings settings)
    {
        var extension = Path.GetExtension(input);

        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        var extensionWithoutDot = extension.Substring(1);

        foreach (var allowedExtension in settings.AllowedFileExtensions)
        {
            if (extensionWithoutDot.Equals(allowedExtension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that the resolved path stays within <see cref="SanitizationSettings.AllowedBaseDirectories"/>,
    /// using canonical-path resolution per CWE-22 recommendations.
    /// </summary>
    /// <param name="input">The input path to check.</param>
    /// <param name="settings">The sanitization settings.</param>
    /// <returns>True if the path is within allowed directories, false otherwise.</returns>
    private static bool IsWithinAllowedDirectories(string input, SanitizationSettings settings)
    {
        if (settings.AllowedBaseDirectories.Length == 0)
        {
            return true;
        }

        try
        {
            if (!Path.IsPathRooted(input))
            {
                if (ContainsDangerousTraversalPatterns(input))
                {
                    return false;
                }

                foreach (var allowedDirectory in settings.AllowedBaseDirectories)
                {
                    var combinedPath = Path.Combine(allowedDirectory, input);
                    var fullPath = Path.GetFullPath(combinedPath);
                    var allowedFullPath = Path.GetFullPath(allowedDirectory);

                    if (IsPathWithinDirectory(fullPath, allowedFullPath))
                    {
                        return true;
                    }
                }

                return false;
            }

            var absolutePath = Path.GetFullPath(input);

            foreach (var allowedDirectory in settings.AllowedBaseDirectories)
            {
                var allowedFullPath = Path.GetFullPath(allowedDirectory);

                if (IsPathWithinDirectory(absolutePath, allowedFullPath))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            // If path resolution fails (e.g. invalid characters the OS rejects), treat as invalid.
            return false;
        }
    }

    /// <summary>
    /// Determines whether <paramref name="candidateFullPath"/> is equal to, or nested within,
    /// <paramref name="allowedFullPath"/>. Both paths must already be canonicalized via
    /// <see cref="Path.GetFullPath(string)"/>. Implemented via prefix comparison rather than
    /// <c>Path.GetRelativePath</c> so the check compiles identically on <c>netstandard2.0</c>
    /// (which lacks that API) and <c>net8.0</c>.
    /// </summary>
    /// <param name="candidateFullPath">The canonicalized candidate path.</param>
    /// <param name="allowedFullPath">The canonicalized allowed base directory.</param>
    /// <returns>True if the candidate path is the allowed directory or a descendant of it.</returns>
    private static bool IsPathWithinDirectory(string candidateFullPath, string allowedFullPath)
    {
        var trimmedAllowed = allowedFullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (string.Equals(candidateFullPath, trimmedAllowed, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var allowedWithSeparator = trimmedAllowed + Path.DirectorySeparatorChar;
        return candidateFullPath.StartsWith(allowedWithSeparator, StringComparison.OrdinalIgnoreCase);
    }
}

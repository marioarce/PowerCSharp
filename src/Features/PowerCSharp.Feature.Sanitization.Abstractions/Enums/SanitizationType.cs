namespace PowerCSharp.Feature.Sanitization.Abstractions.Enums;

/// <summary>
/// Defines the types of sanitization available for data processing.
/// Each type represents a specific security or data integrity concern.
/// </summary>
public enum SanitizationType
{
    /// <summary>
    /// Sanitizes data to prevent log injection attacks by removing or encoding control characters.
    /// Addresses CWE-117: Improper Output Neutralization for Logs.
    /// </summary>
    LogInjection,

    /// <summary>
    /// Sanitizes file paths to prevent directory traversal attacks.
    /// Addresses CWE-22: Improper Limitation of a Pathname to a Restricted Directory.
    /// </summary>
    FilePath,

    /// <summary>
    /// Detects and masks sensitive data in log messages to prevent information disclosure.
    /// Addresses CWE-200: Exposure of Sensitive Information to an Unauthorized Actor.
    /// </summary>
    SensitiveData,

    /// <summary>
    /// Sanitizes regex patterns to prevent Regular Expression Denial of Service (ReDoS) attacks.
    /// Addresses CWE-400: Uncontrolled Resource Consumption and CWE-730: Uncontrolled Recursion.
    /// </summary>
    RegexInjection
}

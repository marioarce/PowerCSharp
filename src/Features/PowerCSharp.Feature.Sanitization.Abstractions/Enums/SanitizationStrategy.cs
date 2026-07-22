namespace PowerCSharp.Feature.Sanitization.Abstractions.Enums;

/// <summary>
/// Defines the strategy used to neutralize control characters during log-injection sanitization.
/// Each strategy represents a different approach to addressing CWE-117 vulnerabilities.
/// </summary>
public enum SanitizationStrategy
{
    /// <summary>
    /// Removes control characters completely from the input.
    /// This is the most aggressive approach and provides clean log output.
    /// Recommended for most logging scenarios where character preservation is not critical.
    /// </summary>
    Remove,

    /// <summary>
    /// Replaces control characters with a space character.
    /// This maintains the original string length while neutralizing injection risks.
    /// Useful when maintaining text structure is important for log parsing.
    /// </summary>
    ReplaceWithSpace,

    /// <summary>
    /// Encodes control characters using HTML entity encoding.
    /// This preserves the original characters in a safe format for display.
    /// Recommended for web-based logging systems where logs may be rendered as HTML.
    /// Addresses both CWE-117 and potential CWE-79 (XSS) in web log viewers.
    /// </summary>
    HtmlEncode,

    /// <summary>
    /// Encodes control characters using URL encoding.
    /// This preserves characters in a format safe for URL-based log systems.
    /// Useful when log data may be transmitted via HTTP parameters or URLs.
    /// </summary>
    UrlEncode,

    /// <summary>
    /// Encodes control characters using JSON escape sequences.
    /// This preserves characters in a format safe for JSON-based logging systems.
    /// Recommended for structured logging where logs are stored or transmitted as JSON.
    /// </summary>
    JsonEncode
}

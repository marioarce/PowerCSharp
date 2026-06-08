using System;
using System.Linq;
using System.Net.Mail;

namespace PowerCSharp.Utilities;

/// <summary>
/// Utility class for common validation operations that return boolean results
/// </summary>
public static class ValidationUtility
{
    /// <summary>
    /// Validates if an email address is in proper format using .NET's built-in MailAddress validation.
    /// This method provides comprehensive email format validation without external dependencies.
    /// </summary>
    /// <param name="email">The email address to validate. Can be null or empty.</param>
    /// <returns>
    /// True if the email address is in valid format; false if null, empty, whitespace, or invalid format.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Validation Criteria:</strong>
    /// - Uses System.Net.Mail.MailAddress for standards-compliant validation
    /// - Validates format according to RFC 5322 email standards
    /// - Handles international domain names (IDN) if supported by framework
    /// </para>
    /// <para>
    /// <strong>Edge Cases Handled:</strong>
    /// - null input: returns false
    /// - empty string: returns false
    /// - whitespace only: returns false
    /// - malformed email addresses: returns false
    /// - valid emails with special characters: returns true
    /// </para>
    /// <para>
    /// <strong>Security Considerations:</strong>
    /// - Only validates format, not email existence
    /// - Does not perform DNS lookups or MX record validation
    /// - Safe for user input validation without external calls
    /// </para>
    /// <strong>Examples:</strong>
    /// <code>
    /// ValidationUtility.IsValidEmail(null); // false
    /// ValidationUtility.IsValidEmail(""); // false
    /// ValidationUtility.IsValidEmail("   "); // false
    /// ValidationUtility.IsValidEmail("invalid-email"); // false
    /// ValidationUtility.IsValidEmail("@domain.com"); // false
    /// ValidationUtility.IsValidEmail("user@"); // false
    /// ValidationUtility.IsValidEmail("user@domain"); // true
    /// ValidationUtility.IsValidEmail("user.name@domain.com"); // true
    /// ValidationUtility.IsValidEmail("user+tag@domain.co.uk"); // true
    /// ValidationUtility.IsValidEmail("user@international-domain.中国"); // true (if supported)
    /// </code>
    /// </remarks>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var addr = new MailAddress(email);
            var result = addr.Address == email;
            return result;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string contains only digits (0-9).
    /// This method provides fast numeric validation without regex overhead.
    /// </summary>
    /// <param name="value">The string to validate for numeric content. Can be null or empty.</param>
    /// <returns>
    /// True if the string contains only digit characters; false if null, empty, whitespace, or contains non-digit characters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Validation Criteria:</strong>
    /// - Only characters '0' through '9' are allowed
    /// - No decimal points, negative signs, or whitespace
    /// - Empty strings are considered invalid (returns false)
    /// </para>
    /// <para>
    /// <strong>Edge Cases Handled:</strong>
    /// - null input: returns false
    /// - empty string: returns false
    /// - whitespace only: returns false
    /// - strings with non-digit characters: returns false
    /// - strings with digits only: returns true
    /// </para>
    /// <para>
    /// <strong>Performance Characteristics:</strong>
    /// - Uses LINQ All() with char.IsDigit for efficient validation
    /// - Short-circuits on first non-digit character
    /// - No regex compilation overhead
    /// </para>
    /// <strong>Examples:</strong>
    /// <code>
    /// ValidationUtility.IsNumeric(null); // false
    /// ValidationUtility.IsNumeric(""); // false
    /// ValidationUtility.IsNumeric("   "); // false
    /// ValidationUtility.IsNumeric("123"); // true
    /// ValidationUtility.IsNumeric("00123"); // true (leading zeros allowed)
    /// ValidationUtility.IsNumeric("12.34"); // false (decimal point not allowed)
    /// ValidationUtility.IsNumeric("-123"); // false (negative sign not allowed)
    /// ValidationUtility.IsNumeric("12a34"); // false (letter not allowed)
    /// ValidationUtility.IsNumeric("12 34"); // false (space not allowed)
    /// </code>
    /// </remarks>
    public static bool IsNumeric(string? value)
    {
        var result = !string.IsNullOrWhiteSpace(value) && value.All(char.IsDigit);
        return result;
    }

    /// <summary>
    /// Validates if a string is a valid absolute URL with HTTP or HTTPS scheme.
    /// This method provides comprehensive URL validation using .NET's built-in Uri validation.
    /// </summary>
    /// <param name="url">The URL string to validate. Can be null or empty.</param>
    /// <returns>
    /// True if the string is a valid absolute URL with HTTP or HTTPS scheme; false otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Validation Criteria:</strong>
    /// - Must be an absolute URL (not relative)
    /// - Must use HTTP or HTTPS scheme only
    /// - Must follow RFC 3986 URI standards
    /// - Uses System.Uri for standards-compliant validation
    /// </para>
    /// <para>
    /// <strong>Edge Cases Handled:</strong>
    /// - null input: returns false
    /// - empty string: returns false
    /// - whitespace only: returns false
    /// - relative URLs: returns false
    /// - non-HTTP/HTTPS schemes: returns false
    /// - malformed URLs: returns false
    /// </para>
    /// <para>
    /// <strong>Security Considerations:</strong>
    /// - Only accepts HTTP and HTTPS schemes for security
    /// - Prevents validation of potentially dangerous schemes (file://, javascript://, etc.)
    /// - Does not perform DNS lookups or connectivity checks
    /// - Safe for user input validation without external calls
    /// </para>
    /// <strong>Examples:</strong>
    /// <code>
    /// ValidationUtility.IsValidUrl(null); // false
    /// ValidationUtility.IsValidUrl(""); // false
    /// ValidationUtility.IsValidUrl("   "); // false
    /// ValidationUtility.IsValidUrl("/relative/path"); // false (relative URL)
    /// ValidationUtility.IsValidUrl("ftp://example.com"); // false (FTP not allowed)
    /// ValidationUtility.IsValidUrl("javascript:alert('xss')"); // false (dangerous scheme)
    /// ValidationUtility.IsValidUrl("not-a-url"); // false (invalid format)
    /// ValidationUtility.IsValidUrl("http://example.com"); // true
    /// ValidationUtility.IsValidUrl("https://example.com"); // true
    /// ValidationUtility.IsValidUrl("https://example.com:8080/path?query=value#fragment"); // true
    /// ValidationUtility.IsValidUrl("https://user:pass@example.com"); // true (credentials allowed)
    /// ValidationUtility.IsValidUrl("https://international-domain.中国"); // true (IDN supported)
    /// </code>
    /// </remarks>
    public static bool IsValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

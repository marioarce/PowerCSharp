using System;
using System.Linq;
using System.Net.Mail;

namespace PowerCSharp.Utilities;

/// <summary>
/// Utility class for common validation operations
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates if an email address is in proper format
    /// </summary>
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
    /// Validates if a string contains only digits
    /// </summary>
    public static bool IsNumeric(string? value)
    {
        var result = !string.IsNullOrWhiteSpace(value) && value.All(char.IsDigit);
        return result;
    }

    /// <summary>
    /// Validates if a string is a valid URL
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

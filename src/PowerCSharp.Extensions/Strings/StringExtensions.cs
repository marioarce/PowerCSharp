using System;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerCSharp.Extensions.Strings;

/// <summary>
/// Extension methods for string manipulation and validation
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets the middle part of a string starting from the specified index.
    /// </summary>
    /// <param name="text">The source string.</param>
    /// <param name="start">The starting index into the string.</param>
    /// <returns>The rightmost part of the string starting at the start position, or empty string if start is invalid.</returns>
    /// <remarks>If start is greater than the length of the string, an empty string is returned.</remarks>
    public static string Mid(this string text, int start)
    {
        if (string.IsNullOrEmpty(text) || start < 0 || start >= text.Length)
        {
            return string.Empty;
        }

        return text.Substring(start);
    }

    /// <summary>
    /// Gets the string with the first character converted to lowercase.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The string with the first character in lowercase, or the original string if null/empty or first character is not uppercase.</returns>
    public static string? FirstCharToLowerCase(this string? input)
    {
        if (!string.IsNullOrEmpty(input) && char.IsUpper(input[0]))
        {
            return input.Length == 1 ? char.ToLower(input[0]).ToString() : char.ToLower(input[0]) + input.Substring(1);
        }

        return input;
    }

    /// <summary>
    /// Converts the string to camel case format.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string converted to camel case.</returns>
    public static string ToCamelCase(this string input)
    {
        // If there are 0 or 1 characters, just return the string.
        if (string.IsNullOrEmpty(input) || input.Length < 2)
        {
            return input ?? string.Empty;
        }

        // Split the string into words.
        var words = input.Split(
            new char[] { },
            StringSplitOptions.RemoveEmptyEntries);

        // Combine the words.
        var result = char.ToLowerInvariant(words[0][0]) + words[0].Substring(1);
        
        for (var i = 1; i < words.Length; i++)
        {
            result +=
                words[i].Substring(0, 1).ToUpper() +
                words[i].Substring(1);
        }

        result = result.FirstCharToLowerCase();

        return result;
    }

    /// <summary>
    /// Normalizes a string to be a valid JSON key.
    /// Applies camelCase transformation and removes invalid characters.
    /// </summary>
    /// <param name="input">The input string to normalize.</param>
    /// <returns>A normalized string suitable for use as a JSON key.</returns>
    public static string NormalizeKey(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        // Remove invalid characters (only allow letters, numbers, and underscores)
        var buffer = new char[input.Length];
        int idx = 0;

        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                buffer[idx++] = ch;
            }
            else
            {
                buffer[idx++] = ' ';
            }
        }

        var result = new string(buffer, 0, idx);
        result = result.ToCamelCase();

        return result;
    }

    /// <summary>
    /// Strips non-ASCII characters from the string.
    /// </summary>
    /// <param name="input">The input string to process.</param>
    /// <returns>The string with only ASCII characters, or the original string if null/empty.</returns>
    public static string AsAscii(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        var result = Encoding.ASCII.GetString(
            Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding(
                    Encoding.ASCII.EncodingName,
                    new EncoderReplacementFallback(string.Empty),
                    new DecoderExceptionFallback()
                    ),
                Encoding.UTF8.GetBytes(input)
            )
        );

        return result;
    }

    /// <summary>
    /// Returns true if the string is a valid absolute HTTP or HTTPS URL.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid URL; otherwise, false.</returns>
    public static bool IsValidUrl(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var result = Uri.TryCreate(input, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        return result;
    }
}

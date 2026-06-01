using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PowerCSharp.Extensions.Strings;

/// <summary>
/// Extension methods for string manipulation and validation
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if a string is null, empty, or contains only whitespace
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if the string is null, empty, or whitespace; otherwise false</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Safely gets a substring without throwing exceptions
    /// </summary>
    /// <param name="value">The source string</param>
    /// <param name="startIndex">The starting index</param>
    /// <param name="length">The length of the substring</param>
    /// <returns>The substring or empty string if parameters are invalid</returns>
    public static string SafeSubstring(this string? value, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(value) || startIndex < 0 || length < 0 || startIndex >= value.Length)
            return string.Empty;

        var actualLength = Math.Min(length, value.Length - startIndex);
        return value.Substring(startIndex, actualLength);
    }

    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized)
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>The title case string</returns>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var words = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }

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

    /// <summary>
    /// Content Token regex pattern: [[ ]].
    /// </summary>
    private const string ContentTokenPattern = @"\[\[(.*?)]]";

    /// <summary>
    /// Determines whether the specified input string contains any content tokens.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns><c>true</c> if the input string contains content tokens; otherwise, <c>false</c>.</returns>
    public static bool ContainsContentToken(this string input)
    {
        var matches = Regex.Match(input, ContentTokenPattern);
        return matches.Success;
    }

    /// <summary>
    /// Removes all whitespace characters from the specified string.
    /// </summary>
    /// <param name="input">The input string containing whitespace.</param>
    /// <returns>The input string without any whitespace characters.</returns>
    public static string RemoveWhitespaces(this string input)
    {
        return Regex.Replace(input, @"\s+", string.Empty);
    }

    /// <summary>
    /// Encodes special characters in the specified string to their HTML entity equivalents.
    /// </summary>
    /// <param name="input">The input string to encode.</param>
    /// <returns>The encoded string with special characters replaced by HTML entities.</returns>
    public static string EncodeCharacters(this string input)
    {
        var result = input?
            .Replace("&", "&#x26;")
            .Replace("<", "&#x3C;")
            .Replace("'", "&#x27;")
            .Replace("\"", "&#x22;")
            .Replace(">", "&#x3E;");

        return result ?? string.Empty;
    }

    /// <summary>
    /// Converts the specified string to a lowercase, hyphen-separated format.
    /// Replaces spaces and underscores with hyphens and converts all characters to lowercase.
    /// </summary>
    /// <param name="input">The input string to hyphenate.</param>
    /// <returns>
    /// A lowercase string with spaces and underscores replaced by hyphens.
    /// If the input is null or empty, returns the input unchanged.
    /// </returns>
    public static string Hyphenate(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        return input.Replace(" ", "-").Replace("_", "-").ToLowerInvariant();
    }

    /// <summary>
    /// Validate if the string given contains a valid JSON
    /// </summary>
    /// <param name="input">A System.String that contains JSON</param>
    /// <returns><c>true</c> if the string is a valid JSON; otherwise <c>false</c></returns>
    public static bool IsValidJson(this string input)
    {
        return IsValidJson(input, out _);
    }

    /// <summary>
    /// Validate if the string given contains a valid JSON
    /// </summary>
    /// <param name="input">A System.String that contains JSON</param>
    /// <param name="exception">When this method returns false, contains the exception that describes why the JSON is invalid; otherwise, null.</param>
    /// <returns><c>true</c> if the string is a valid JSON; otherwise <c>false</c></returns>
    public static bool IsValidJson(this string input, out Exception exception)
    {
        var inputCopy = input;
        exception = null;

        if (string.IsNullOrEmpty(inputCopy))
        {
            exception = new Exception("JsonException: The input is null or empty");
            return false;
        }

        inputCopy = inputCopy.Trim();

        bool IsWellFormed(out Exception exception)
        {
            exception = null;

            try
            {
                JsonDocument.Parse(inputCopy);
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }

            return true;
        }

        var result = ((inputCopy.StartsWith("{") && inputCopy.EndsWith("}"))
                || (inputCopy.StartsWith("[") && inputCopy.EndsWith("]")))
               && IsWellFormed(out exception);

        if (!result && exception == null)
        {
            exception = new Exception("JsonException: Invalid or not well-formed Json");
        }
        if (exception != null)
        {
            exception.Data["jsonString"] = inputCopy;
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified string is a valid GUID.
    /// Attempts to parse the input string as a GUID and returns the result.
    /// </summary>
    /// <param name="input">The string to validate as a GUID.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed <see cref="Guid"/> value if the input is valid; otherwise, <see cref="Guid.Empty"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the input string is a valid GUID; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidGuid(this string input, out Guid result)
    {
        // Guid.TryParse attempts to convert a string representation of a GUID to its Guid equivalent.
        // It returns true if the conversion is successful, and false otherwise.
        // The 'out Guid result' parameter will contain the parsed GUID if successful,
        // or a default Guid value (Guid.Empty) if unsuccessful.
        return Guid.TryParse(input, out result);
    }

    /// <summary>
    /// Validate if the string given contains only digits
    /// </summary>
    /// <param name="input">The input string to validate.</param>
    /// <returns><c>true</c> if the string contains only digits; otherwise, <c>false</c>.</returns>
    public static bool IsDigitsOnly(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        foreach (char c in input)
        {
            if (c < '0' || c > '9')
            {
                return false;
            }
        }

        return true;
    }
}

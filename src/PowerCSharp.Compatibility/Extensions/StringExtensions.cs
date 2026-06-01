using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace PowerCSharp.Compatibility.Extensions;

/// <summary>
/// Provides extension methods for working with strings.
/// Includes utilities for content token detection, query parameter manipulation, whitespace removal, and character encoding.
/// </summary>
public static class StringExtensions
{
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
    /// Appends query parameters to the specified URL.
    /// </summary>
    /// <param name="url">The original URL.</param>
    /// <param name="parameters">A dictionary of query parameters to append.</param>
    /// <returns>The URL with the parameters appended.</returns>
    public static string AppendQueryParameters(this string url, IDictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        if (parameters == null || parameters.Count == 0)
        {
            return url;
        }

        var query = HttpUtility.ParseQueryString(url);
        foreach (var parameter in parameters)
        {
            query.Add(parameter.Key, parameter.Value);
        }

        var outputUrl = HttpUtility.UrlDecode(query.ToString());
        var rex = new Regex("&", RegexOptions.IgnoreCase);

        return outputUrl.IndexOf('?') == -1 ? rex.Replace(outputUrl, "?", 1) : outputUrl;
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

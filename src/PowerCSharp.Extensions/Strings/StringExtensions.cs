using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace PowerCSharp.Extensions.Strings;

/// <summary>
/// Extension methods for string manipulation and validation
/// </summary>
public static partial class StringExtensions
{
#if NET8_0_OR_GREATER
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
#else
    private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
#endif

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"<[^>]*>|&.*?;")]
    private static partial Regex HtmlRegex();
#else
    private static readonly Regex HtmlRegex = new Regex(@"<[^>]*>|&.*?;", RegexOptions.Compiled);
#endif

#if NET8_0_OR_GREATER
    [GeneratedRegex("[^0-9]")]
    private static partial Regex NonDigitRegex();
#else
    private static readonly Regex NonDigitRegex = new Regex("[^0-9]", RegexOptions.Compiled);
#endif

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"(\d{3})(\d{3})(\d{4})")]
    private static partial Regex PhoneRegex();
#else
    private static readonly Regex PhoneRegex = new Regex(@"(\d{3})(\d{3})(\d{4})", RegexOptions.Compiled);
#endif

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"^(?!.)(""([^""\r\\]|\\[""\r\\])*""|"
        + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
        + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$")]
    private static partial Regex EmailRegex();
#else
    private static readonly Regex EmailRegex = new Regex(@"^(?!.)(""([^""\r\\]|\\[""\r\\])*""|"
        + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
        + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
#endif

    private static Regex GetWhitespaceRegex() =>
#if NET8_0_OR_GREATER
        WhitespaceRegex();
#else
        WhitespaceRegex;
#endif

    private static Regex GetHtmlRegex() =>
#if NET8_0_OR_GREATER
        HtmlRegex();
#else
        HtmlRegex;
#endif

    private static Regex GetNonDigitRegex() =>
#if NET8_0_OR_GREATER
        NonDigitRegex();
#else
        NonDigitRegex;
#endif

    private static Regex GetPhoneRegex() =>
#if NET8_0_OR_GREATER
        PhoneRegex();
#else
        PhoneRegex;
#endif

    private static Regex GetEmailRegex() =>
#if NET8_0_OR_GREATER
        EmailRegex();
#else
        EmailRegex;
#endif

    /// <summary>
    /// Checks if a string is null, empty, or contains only whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    /// True if the string is null, empty, or contains only whitespace characters;
    /// false if the string contains non-whitespace characters.
    /// </returns>
    /// <remarks>
    /// This method provides a null-safe wrapper around string.IsNullOrWhiteSpace().
    /// It handles null values gracefully and returns true for null, empty string (""),
    /// or strings containing only whitespace characters (spaces, tabs, newlines, etc.).
    /// 
    /// Edge cases handled:
    /// - null input: returns true
    /// - empty string: returns true  
    /// - whitespace only: returns true
    /// - string with content: returns false
    /// 
    /// Example:
    /// <code>
    /// string? nullString = null;
    /// string emptyString = "";
    /// string whitespaceString = "   \t\n   ";
    /// string contentString = "hello";
    /// 
    /// nullString.IsNullOrWhiteSpace(); // true
    /// emptyString.IsNullOrWhiteSpace(); // true
    /// whitespaceString.IsNullOrWhiteSpace(); // true
    /// contentString.IsNullOrWhiteSpace(); // false
    /// </code>
    /// </remarks>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Safely gets a substring without throwing exceptions.
    /// This method provides a null-safe, bounds-safe alternative to string.Substring().
    /// </summary>
    /// <param name="value">The source string to extract from.</param>
    /// <param name="startIndex">The starting index (0-based). Must be non-negative.</param>
    /// <param name="length">The desired length of the substring. Must be non-negative.</param>
    /// <returns>
    /// The substring if parameters are valid; otherwise returns an empty string.
    /// The returned string may be shorter than requested if the parameters exceed the string length.
    /// </returns>
    /// <remarks>
    /// This method handles all edge cases gracefully without throwing exceptions:
    /// - null or empty input string: returns empty string
    /// - negative startIndex or length: returns empty string
    /// - startIndex >= string length: returns empty string
    /// - length exceeds available characters: returns substring up to end of string
    /// 
    /// Security considerations:
    /// - This method does not throw exceptions, making it safe for use in performance-critical paths
    /// - Input validation prevents potential index out of range exceptions
    /// 
    /// Edge cases:
    /// <code>
    /// string? nullString = null;
    /// nullString.SafeSubstring(0, 5); // "" (empty string)
    /// 
    /// string shortString = "abc";
    /// shortString.SafeSubstring(1, 10); // "bc" (returns available characters)
    /// shortString.SafeSubstring(5, 2); // "" (start index out of range)
    /// shortString.SafeSubstring(0, -1); // "" (negative length)
    /// shortString.SafeSubstring(-1, 2); // "" (negative start index)
    /// </code>
    /// 
    /// Performance: This method performs bounds checking before substring extraction,
    /// making it slightly slower than direct string.Substring() but safer for user input.
    /// </remarks>
    public static string SafeSubstring(this string? value, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(value) || startIndex < 0 || length < 0 || startIndex >= value!.Length)
        {
            return string.Empty;
        }

        var actualLength = Math.Min(length, value.Length - startIndex);
        return value.Substring(startIndex, actualLength);
    }

    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized).
    /// Handles null/empty input gracefully and preserves word boundaries.
    /// </summary>
    /// <param name="value">The string to convert to title case.</param>
    /// <returns>
    /// The title case string, or empty string if input is null or empty.
    /// Each word's first character is capitalized, subsequent characters are lowercase.
    /// </returns>
    /// <remarks>
    /// This method handles various edge cases:
    /// - null input: returns empty string
    /// - empty string: returns empty string
    /// - single word: capitalizes first letter only
    /// - multiple words: capitalizes first letter of each word
    /// - multiple spaces: treats consecutive spaces as word separators
    /// - mixed case input: normalizes to proper title case
    /// 
    /// Edge case examples:
    /// <code>
    /// string? nullString = null;
    /// nullString.ToTitleCase(); // ""
    /// 
    /// string emptyString = "";
    /// emptyString.ToTitleCase(); // ""
    /// 
    /// string singleWord = "hello";
    /// singleWord.ToTitleCase(); // "Hello"
    /// 
    /// string multipleWords = "hello world";
    /// multipleWords.ToTitleCase(); // "Hello World"
    /// 
    /// string mixedCase = "hELLo wORLD";
    /// mixedCase.ToTitleCase(); // "Hello World"
    /// 
    /// string multipleSpaces = "hello   world";
    /// multipleSpaces.ToTitleCase(); // "Hello World" (extra spaces collapsed)
    /// </code>
    /// 
    /// Note: This method uses StringSplitOptions.RemoveEmptyEntries, so multiple
    /// consecutive spaces are treated as single word separators.
    /// </remarks>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var words = value!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(input) && char.IsUpper(input![0]))
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
    public static string? ToCamelCase(this string input)
    {
        // If there are 0 or 1 characters, just return the string.
        if (string.IsNullOrEmpty(input) || input.Length < 2)
        {
            return input ?? string.Empty;
        }

        // Split the string into words.
        var words = input.Split(
            new char[] { },
            StringSplitOptions.RemoveEmptyEntries
        );

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
    public static string? NormalizeKey(this string input)
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
    public static bool IsValidJson(this string input, out Exception? exception)
    {
        var inputCopy = input;
        exception = null;

        if (string.IsNullOrEmpty(inputCopy))
        {
            exception = new Exception("JsonException: The input is null or empty");
            return false;
        }

        inputCopy = inputCopy.Trim();

        bool IsWellFormed(out Exception? exception)
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

    /// <summary>
    /// Replaces all the whitespaces with the specified replacement string.
    /// </summary>
    /// <param name="input">The input string containing whitespace.</param>
    /// <param name="replacement">The string to replace for all the whitespaces.</param>
    /// <returns>The input string with all whitespace replaced by the replacement string.</returns>
    public static string ReplaceWhitespace(this string input, string replacement)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return GetWhitespaceRegex().Replace(input, replacement);
    }

    /// <summary>
    /// Formats the string as a Price, including the dollar sign as prefix.
    /// Handles decimal conversion and ceiling rounding.
    /// </summary>
    /// <param name="input">The input string containing a numeric value.</param>
    /// <returns>A formatted price string with dollar sign and proper formatting, or the original string if parsing fails.</returns>
    public static string FormatPrice(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        if (input.StartsWith("$"))
        {
            return input;
        }

        try
        {
            var result = $"{Math.Ceiling(Convert.ToDecimal(input)):N0}";
            return $"${result}";
        }
        catch (Exception)
        {
            return input;
        }
    }

    /// <summary>
    /// Converts the string representation of a number to its Decimal equivalent.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="defaultValue">Default value if the input value is invalid.</param>
    /// <returns>A decimal number that is equivalent to input value.</returns>
    public static decimal ParseToDecimal(this string input, decimal defaultValue)
    {
        var result = defaultValue;
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }
        
        // Remove non-numeric characters except decimal point
        input = string.Concat(input?.Where(c => char.IsNumber(c) || c == '.') ?? "");
        _ = decimal.TryParse(input, out result);

        return result;
    }

    /// <summary>
    /// Converts the string representation of a number to its Double equivalent.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="defaultValue">Default value if the input value is invalid.</param>
    /// <returns>A double number that is equivalent to input value.</returns>
    public static double ParseToDouble(this string input, double defaultValue)
    {
        var result = defaultValue;
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }

        // Remove non-numeric characters except decimal point
        input = string.Concat(input?.Where(c => char.IsNumber(c) || c == '.') ?? "");
        _ = double.TryParse(input, out result);

        return result;
    }

    /// <summary>
    /// Converts the string representation of a number to its Integer equivalent.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="defaultValue">Default value if the input value is invalid.</param>
    /// <returns>An integer number that is equivalent to input value.</returns>
    public static int ParseToInt(this string input, int defaultValue)
    {
        var result = defaultValue;
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }

        // Remove non-numeric characters except decimal point
        input = string.Concat(input?.Where(c => char.IsNumber(c) || c == '.') ?? "");
        _ = int.TryParse(input, out result);

        return result;
    }

    /// <summary>
    /// Returns <c>true</c> when the string value is "1" or "true", otherwise returns <c>false</c>.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="defaultValue">Default value if the input value is invalid.</param>
    /// <returns>Boolean representation of the string.</returns>
    public static bool ParseToBoolean(this string? input, bool defaultValue)
    {
        var result = defaultValue;
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }

        input = input?.ToLower();

        if (input != null)
        {
            result = input.Equals("1") || input.Equals("true");
        }

        return result;
    }

    /// <summary>
    /// Converts the string representation of numbers to a Phone format equivalent.
    /// </summary>
    /// <returns>Phone format string under the pattern ###-###-####.</returns>
    public static string ParseToPhoneNumber(this string input)
    {
        var result = string.Empty;

        if (!string.IsNullOrEmpty(input))
        {
            result = GetNonDigitRegex().Replace(input, "");
            result = GetPhoneRegex().Replace(result, "$1-$2-$3");
        }

        return result;
    }

    /// <summary>
    /// Formats a 9-digit zip code with a hyphen (#####-####).
    /// </summary>
    /// <param name="input">The input string containing a 9-digit zip code.</param>
    /// <returns>Formatted zip code with hyphen, or original string if not 9 digits.</returns>
    public static string ParseTo9DigitZipCode(this string input)
    {
        var result = input;

        if (!string.IsNullOrEmpty(result) && result.Length == 9)
        {
            if (int.TryParse(result, out int zip))
            {
                result = $"{zip:#####-####}";
            }
        }

        return result;
    }

    /// <summary>
    /// Removes the dash from a 9-digit zip code, converting it to 5-digit format.
    /// </summary>
    /// <param name="zipCode">The zip code string to convert.</param>
    /// <returns>5-digit zip code, or the first non-empty segment if available.</returns>
    public static string Convert9DigitZipTo5Digit(this string zipCode)
    {
        return zipCode.Split('-')
            .FirstOrDefault(x => !string.IsNullOrEmpty(x))
            ?? zipCode;
    }

    /// <summary>
    /// Returns a copy of the string after removing Html Tags and Entities like &reg; or &nbsp;.
    /// </summary>
    /// <param name="input">The input string containing HTML.</param>
    /// <returns>The string with HTML tags and entities removed.</returns>
    public static string RemoveHtml(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var inputStringBuilder = new StringBuilder(input);
        inputStringBuilder = inputStringBuilder.Replace("&lt;", "<");
        inputStringBuilder = inputStringBuilder.Replace("&gt;", ">");

        var result = inputStringBuilder.ToString();
        result = GetHtmlRegex().Replace(result, string.Empty);
        result = result.Trim();

        return result;
    }

    /// <summary>
    /// Determines if the string is a valid email address format.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>true if the string is a valid email address; otherwise false.</returns>
    public static bool IsEmail(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        return GetEmailRegex()
            .IsMatch(input);
    }

    /// <summary>
    /// Return true if the input is a well-formatted JWT Token. Refer https://jwt.io/introduction
    /// </summary>
    /// <param name="input">The string to validate as JWT.</param>
    /// <returns>true if the string appears to be a JWT token; otherwise false.</returns>
    /// <remarks>
    /// This is a basic structural validation only. For proper JWT validation, use a JWT library.
    /// </remarks>
    public static bool IsJwt(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var parts = input.Split('.');
        // The JWT is composed of three parts: header, payload, signature
        return parts.Length == 3;
    }

    /// <summary>
    /// Validates if the string given contains a valid XML
    /// </summary>
    /// <param name="input">A System.String that contains XML</param>
    /// <returns><c>true</c> if the string is a valid XML; otherwise <c>false</c></returns>
    public static bool IsValidXml(this string input)
    {
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(input);
        }
        catch
        {
            return false;
        }

        return true;
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Mask the source string with the mask char given and using a fixed visibility
    /// </summary>
    /// <param name="value">The string to mask.</param>
    /// <param name="mask">The character to use for masking.</param>
    /// <returns>The masked string.</returns>
    public static string Mask(this string value, char mask)
    {
        const int visibility = 4;
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var visibleCharLength = value.Length / visibility;
        return Mask(value, visibleCharLength, mask);
    }

    /// <summary>
    /// Mask the source string with the mask char given
    /// </summary>
    /// <param name="value">The string to mask.</param>
    /// <param name="visibleCharLength">Number of characters to keep visible at start and end.</param>
    /// <param name="mask">The character to use for masking.</param>
    /// <returns>The masked string.</returns>
    public static string Mask(this string value, int visibleCharLength, char mask)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value?.Length <= (visibleCharLength * 2))
        {
            var masked = Enumerable.Repeat(mask, value.Length);
            var result = string.Concat(masked);
            return result;
        }
        else
        {
            var masked = Enumerable.Repeat(mask, value!.Length - (visibleCharLength * 2));
            var prefix = value![..visibleCharLength];
            var maskedString = string.Concat(masked);
            var suffix = value[^visibleCharLength..];

            var result = $"{prefix}{maskedString}{suffix}";
            return result;
        }
    }
#endif

    /// <summary>
    /// Converts the given string to an integer. 
    /// If conversion fails, returns the specified default value.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The value to return if conversion fails.</param>
    /// <returns>The converted integer or the default value.</returns>
    public static int ToIntOrDefault(this string? value, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Converts the given string to a double value using invariant culture.
    /// If conversion fails or the input is null/empty, returns the specified default value.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The value to return if conversion fails.</param>
    /// <returns>The converted double or the default value.</returns>
    public static double ToDoubleOrDefault(this string? value, double defaultValue = 0)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Returns the string if it is NOT null or empty; otherwise returns the fallback value.
    /// </summary>
    /// <param name="value">The primary string value.</param>
    /// <param name="fallback">The fallback value to use if primary is null/empty.</param>
    /// <returns>The primary string or fallback value.</returns>
    public static string OrFallback(this string value, string? fallback)
    {
        return string.IsNullOrEmpty(value)
            ? fallback
            : value;
    }

    /// <summary>
    /// Converts a string to byte array, trying Base64 decoding first, then falling back to UTF-8 encoding.
    /// This is useful for secrets that may be stored in either Base64 or plain text format.
    /// </summary>
    /// <param name="value">The string to convert to bytes</param>
    /// <returns>The byte array representation of the string, or empty array if value is null/empty</returns>
    public static byte[] ToBytesFromBase64OrUtf8(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<byte>();
        }

        try
        {
            // Try Base64 decode first
            return Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            // If Base64 decode fails, treat as plain text UTF-8
            return Encoding.UTF8.GetBytes(value);
        }
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Normalizes a GUID string by removing all non-alphanumeric characters and converting it to lowercase.
    /// </summary>
    /// <param name="guid">The GUID string to normalize.</param>
    /// <returns>A normalized GUID string containing only alphanumeric characters.</returns>
    public static string NormalizeGuid(this string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
        {
            return string.Empty;
        }

        // First pass: count valid chars
        var count = 0;

        foreach (var c in guid)
        {
            if (char.IsLetterOrDigit(c))
            {
                count++;
            }
        }

        var result = string.Create(count, guid, (chars, state) =>
        {
            var pos = 0;

            foreach (var c in state)
            {
                if (char.IsLetterOrDigit(c))
                {
                    chars[pos++] = char.ToLowerInvariant(c);
                }
            }
        });

        return result;
    }
#endif

    /// <summary>
    /// Validates if the string has a valid JWT format (header.payload.signature).
    /// </summary>
    /// <param name="input">The string to validate as JWT format.</param>
    /// <returns><c>true</c> if the string has valid JWT format; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// This method validates the basic JWT structure by checking:
    /// - The string contains exactly three parts separated by dots
    /// - Each part is a valid Base64URL-encoded string
    /// - The string is not null or empty
    /// This is a format validation only and does not validate the JWT signature or claims.
    /// </remarks>
    public static bool IsValidJwtFormat(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var parts = input.Split('.');

        if (parts.Length != 3)
        {
            return false;
        }

        // Validate each part is valid Base64URL
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
            {
                return false;
            }

            // Try to decode as Base64URL to validate format
            try
            {
                // Add padding if needed for Base64URL decoding
                var base64 = part
                    .Replace('-', '+')
                    .Replace('_', '/');

                switch (base64.Length % 4)
                {
                    case 2:
                        base64 += "==";
                        break;
                    case 3:
                        base64 += "=";
                        break;
                    case 1:
                        return false; // Invalid padding
                }

                Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        return true;
    }
}

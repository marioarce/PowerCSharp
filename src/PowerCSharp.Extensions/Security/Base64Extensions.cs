using System;
using System.Text;

namespace PowerCSharp.Extensions.Security;

/// <summary>
/// Extension methods for Base64 URL-safe encoding and decoding operations.
/// </summary>
/// <remarks>
/// URL-safe Base64 encoding replaces '+' with '-' and '/' with '_' and removes padding characters.
/// This makes the resulting string safe to use in URLs and file names without requiring additional encoding.
/// </remarks>
public static class Base64Extensions
{
    private static readonly char[] Padding = { '=' };

    /// <summary>
    /// Converts a byte array to a URL-safe Base64 encoded string.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <returns>A URL-safe Base64 encoded string, or null if the input is null.</returns>
    /// <remarks>
    /// This method performs URL-safe encoding by:
    /// 1. Converting to standard Base64
    /// 2. Removing padding characters (=)
    /// 3. Replacing '+' with '-'
    /// 4. Replacing '/' with '_'
    /// </remarks>
    public static string? ToBase64UrlSafe(this byte[]? data)
    {
        if (data == null)
        {
            return null;
        }

        return Convert.ToBase64String(data)
            .TrimEnd(Padding)
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Converts a string to a URL-safe Base64 encoded string using UTF-8 encoding.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <returns>A URL-safe Base64 encoded string, or null if the input is null.</returns>
    public static string? ToBase64UrlSafe(this string? input)
    {
        if (input == null)
        {
            return null;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        return bytes.ToBase64UrlSafe();
    }

    /// <summary>
    /// Decodes a URL-safe Base64 encoded string back to its original string representation.
    /// </summary>
    /// <param name="encodedString">The URL-safe Base64 encoded string to decode.</param>
    /// <returns>The decoded string using UTF-8 encoding, or null if the input is null.</returns>
    /// <remarks>
    /// This method reverses the URL-safe encoding by:
    /// 1. Replacing '-' with '+'
    /// 2. Replacing '_' with '/'
    /// 3. Adding appropriate padding characters
    /// 4. Decoding from Base64
    /// 5. Converting back to string using UTF-8
    /// </remarks>
    /// <exception cref="FormatException">Thrown when the input string is not a valid Base64 format.</exception>
    public static string? FromBase64UrlSafe(this string? encodedString)
    {
        if (encodedString == null)
        {
            return null;
        }

        var incoming = encodedString.Replace('_', '/').Replace('-', '+');
        
        // Add padding characters if needed
        switch (incoming.Length % 4)
        {
            case 2:
                incoming += "==";
                break;
            case 3:
                incoming += "=";
                break;
        }

        var bytes = Convert.FromBase64String(incoming);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Decodes a URL-safe Base64 encoded string back to its original byte array representation.
    /// </summary>
    /// <param name="encodedString">The URL-safe Base64 encoded string to decode.</param>
    /// <returns>The decoded byte array, or null if the input is null.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not a valid Base64 format.</exception>
    public static byte[]? FromBase64UrlSafeToBytes(this string? encodedString)
    {
        if (encodedString == null)
        {
            return null;
        }

        var incoming = encodedString.Replace('_', '/').Replace('-', '+');
        
        // Add padding characters if needed
        switch (incoming.Length % 4)
        {
            case 2:
                incoming += "==";
                break;
            case 3:
                incoming += "=";
                break;
        }

        return Convert.FromBase64String(incoming);
    }

    /// <summary>
    /// Checks if input is a well-formatted Base64 string.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if input is a well-formed Base64 encoded string.</returns>
    public static bool IsBase64Encoded(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // Base 64 encoded strings have length divisible by 4 (e.g. 4, 8, 12)
        if ((input.Length % 4 != 0))
        {
            return false;
        }

        try
        {
            var decodedBytes = Convert.FromBase64String(input);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);

            if (decodedString == null)
            {
                return false;
            }

            // Check for invalid UTF-8 sequences
            return !decodedString.Contains("\0");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets input as Base64 decoded string value.
    /// </summary>
    /// <returns>Base64 decoded representation or original input if not valid Base64.</returns>
    public static string DecodeFromBase64(this string input)
    {
        if (!input.IsBase64Encoded())
        {
            return input;
        }

        try
        {
            var decodedBytes = Convert.FromBase64String(input);
            var originalMessage = Encoding.UTF8.GetString(decodedBytes);
            return originalMessage;
        }
        catch (Exception)
        {
            return input;
        }
    }

    /// <summary>
    /// Gets input Base64 encoded value.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <returns>Base64 encoded representation or original input if encoding fails.</returns>
    public static string ToBase64Encoded(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var result = Convert.ToBase64String(bytes);
            return result;
        }
        catch (Exception)
        {
            return input;
        }
    }
}

using System;
using System.Linq;

namespace PowerCSharp.Core;

/// <summary>
/// Core extension methods for string manipulation and validation
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
}

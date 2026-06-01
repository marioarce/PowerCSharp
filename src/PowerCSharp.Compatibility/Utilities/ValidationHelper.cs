using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Xml;
using PowerCSharp.Compatibility.Utilities.Attributes;

namespace PowerCSharp.Compatibility.Utilities;

/// <summary>
/// Utility class for common validation operations and assertions
/// </summary>
public static class ValidationHelper
{
    #region Assertion Methods

    /// <summary>
    /// Asserts that the arguments are not null.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="argumentName">Name of the argument.</param>
    /// <exception cref="ArgumentNullException">Thrown if the argument is null.</exception>
    public static void ArgumentNotNull([ValidatedNotNull] object argument, string argumentName)
    {
        if (argument != null)
        {
            return;
        }

        if (argumentName != null)
        {
            throw new ArgumentNullException(argumentName);
        }

        throw new ArgumentNullException();
    }

    /// <summary>
    /// Asserts that the arguments are not null or empty.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="argumentName">Name of the argument.</param>
    /// <exception cref="ArgumentNullException">Null ids are not allowed.</exception>
    /// <exception cref="ArgumentException">Empty strings are not allowed.</exception>
    public static void ArgumentNotNullOrEmpty([ValidatedNotNull] string argument, string argumentName)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            return;
        }

        if (argument == null)
        {
            if (argumentName != null)
            {
                throw new ArgumentNullException(argumentName, "Null ids are not allowed.");
            }

            throw new ArgumentNullException();
        }

        if (argumentName != null)
        {
            throw new ArgumentException("Empty strings are not allowed.", argumentName);
        }

        throw new ArgumentException("Empty strings are not allowed.");
    }

    /// <summary>
    /// Asserts that the specified condition is false.
    /// </summary>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsFalse(bool condition, [Localizable(false)] string message)
    {
        ArgumentNotNull(message, "message");
        if (condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The message.</param>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsNotNull(object value, [Localizable(false)] string message)
    {
        if (value == null)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Determines whether the specified condition is true.
    /// </summary>
    /// <param name="condition">if set to true th condition is true.</param>
    /// <param name="message">The message.</param>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsTrue(bool condition, [Localizable(false)] string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Results the not null.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    public static T ResultNotNull<T>(T result, [Localizable(false)] string message) where T : class
    {
        ArgumentNotNullOrEmpty(message, "message");
        IsNotNull(result, message);
        return result;
    }

    #endregion

    #region Validation Methods

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

    #endregion
}

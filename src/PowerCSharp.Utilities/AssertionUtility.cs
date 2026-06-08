using System;
using System.ComponentModel;
using System.Xml;
using PowerCSharp.Utilities.Attributes;

namespace PowerCSharp.Utilities;

/// <summary>
/// Utility class for common assertion operations that throw exceptions
/// </summary>
public static class AssertionUtility
{
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
    /// Asserts that the arguments are not null.
    /// </summary>
    /// <param name="argument">The argument value.</param>
    /// <param name="getArgumentName">The delegate used to get the parameter name.</param>
    /// <exception cref="ArgumentNullException">Thrown if the argument is null.</exception>
    public static void ArgumentNotNull([ValidatedNotNull] object argument, Func<string> getArgumentName)
    {
        if (argument != null)
        {
            return;
        }

        string text = getArgumentName();
        if (text != null)
        {
            throw new ArgumentNullException(text);
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
    /// Asserts that the arguments are not null or empty.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="getArgumentName">Delegate for getting the argument name.</param>
    /// <exception cref="ArgumentNullException">Null ids are not allowed.</exception>
    /// <exception cref="ArgumentException">Empty strings are not allowed.</exception>
    public static void ArgumentNotNullOrEmpty([ValidatedNotNull] string argument, Func<string> getArgumentName)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            return;
        }

        string text = getArgumentName();
        if (argument == null)
        {
            if (text != null)
            {
                throw new ArgumentNullException(text, "Null ids are not allowed.");
            }

            throw new ArgumentNullException();
        }

        if (text != null)
        {
            throw new ArgumentException("Empty strings are not allowed.", text);
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
    /// Asserts that the specified condition is false.
    /// </summary>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsFalse(bool condition, Func<string> getMessage)
    {
        if (!condition)
        {
            return;
        }

        string text = getMessage();
        if (text != null)
        {
            throw new InvalidOperationException(text);
        }

        throw new InvalidOperationException();
    }

    /// <summary>
    /// Asserts that the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void IsFalse(bool condition, string format, params object[] args)
    {
        if (condition)
        {
            string message = Format(format, args);
            IsFalse(condition, message);
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
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The arg1.</param>
    public static void IsNotNull(object value, string format, string arg1)
    {
        if (value == null)
        {
            string message = string.Format(format, arg1);
            IsNotNull(value, message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The arg1.</param>
    /// <param name="arg2">The arg2.</param>
    public static void IsNotNull(object value, string format, string arg1, string arg2)
    {
        if (value == null)
        {
            string message = string.Format(format, arg1, arg2);
            IsNotNull(value, message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The arg1.</param>
    /// <param name="arg2">The arg2.</param>
    /// <param name="arg3">The arg3.</param>
    public static void IsNotNull(object value, string format, string arg1, string arg2, string arg3)
    {
        if (value == null)
        {
            string message = string.Format(format, arg1, arg2, arg3);
            IsNotNull(value, message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void IsNotNull(object value, string format, params object[] args)
    {
        if (value == null)
        {
            string message = Format(format, args);
            IsNotNull(value, message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The object type.</param>
    public static void IsNotNull(object value, Type type)
    {
        if (value == null)
        {
            IsNotNull(value, type, string.Empty);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    public static void IsNotNull(object value, Type type, string format, string arg1)
    {
        if (value == null)
        {
            string text = GetTypeMessage(type);
            if (format.Length > 0)
            {
                text = text + " Additional information: " + string.Format(format, arg1);
            }

            IsNotNull(value, text);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public static void IsNotNull(object value, Type type, string format, string arg1, string arg2)
    {
        if (value == null)
        {
            string text = GetTypeMessage(type);
            if (format.Length > 0)
            {
                text = text + " Additional information: " + string.Format(format, arg1, arg2);
            }

            IsNotNull(value, text);
        }
    }

    /// <summary>
    /// Asserts that the specified value is not null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void IsNotNull(object value, Type type, string format, params object[] args)
    {
        if (value == null)
        {
            string text = GetTypeMessage(type);
            if (format.Length > 0)
            {
                text = text + " Additional information: " + Format(format, args);
            }

            IsNotNull(value, text);
        }
    }

    /// <summary>
    /// Asserts that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The message.</param>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsNotNullOrEmpty(string value, [Localizable(false)] string message)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Asserts that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    public static void IsNotNullOrEmpty(string value, string format, string arg1)
    {
        if (string.IsNullOrEmpty(value))
        {
            string message = string.Format(format, arg1);
            IsNotNullOrEmpty(value, message);
        }
    }

    /// <summary>
    /// Asserts that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The args.</param>
    public static void IsNotNullOrEmpty(string value, string format, params object[] args)
    {
        if (string.IsNullOrEmpty(value))
        {
            string message = Format(format, args);
            IsNotNullOrEmpty(value, message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="message">The message.</param>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsNull(object value, [Localizable(false)] string message)
    {
        if (value != null)
        {
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Asserts that the specified value is null.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void IsNull(object value, string format, params object[] args)
    {
        if (value != null)
        {
            string message = Format(format, args);
            IsNull(value, message);
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
    /// Determines whether the specified condition is true.
    /// </summary>
    /// <param name="condition">if set to true the condition is true.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    public static void IsTrue(bool condition, string format, string arg1)
    {
        if (!condition)
        {
            string message = string.Format(format, arg1);
            IsTrue(condition: false, message);
        }
    }

    /// <summary>
    /// Determines whether the specified condition is true.
    /// </summary>
    /// <param name="condition">if set to true the condition is true.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public static void IsTrue(bool condition, string format, string arg1, string arg2)
    {
        if (!condition)
        {
            string message = string.Format(format, arg1, arg2);
            IsTrue(condition: false, message);
        }
    }

    /// <summary>
    /// Determines whether the specified condition is true.
    /// </summary>
    /// <param name="condition">if set to true th condition is true.</param>
    /// <param name="getMessage">The get message delegate.</param>
    /// <exception cref="InvalidOperationException">InvalidOperationException.</exception>
    public static void IsTrue(bool condition, Func<string> getMessage)
    {
        if (condition)
        {
            return;
        }

        string text = getMessage();
        if (text != null)
        {
            throw new InvalidOperationException(text);
        }

        throw new InvalidOperationException();
    }

    /// <summary>
    /// Determines whether the specified condition is true.
    /// </summary>
    /// <param name="condition">if set to true the condition is true.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    public static void IsTrue(bool condition, string format, params object[] args)
    {
        if (!condition)
        {
            string message = Format(format, args);
            IsTrue(condition: false, message);
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

    /// <summary>
    /// Results the not null.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    /// <typeparam name="T"></typeparam>
    public static T ResultNotNull<T>(T result, string format, string arg1) where T : class
    {
        ArgumentNotNullOrEmpty(format, "format");
        if (result == null)
        {
            format = string.Format(format, arg1);
            IsNotNull(result, format);
        }

        return result;
    }

    /// <summary>
    /// Results the not null.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="format">The format.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <typeparam name="T"></typeparam>
    public static T ResultNotNull<T>(T result, string format, string arg1, string arg2) where T : class
    {
        ArgumentNotNullOrEmpty(format, "format");
        if (result == null)
        {
            format = string.Format(format, arg1, arg2);
            IsNotNull(result, format);
        }

        return result;
    }

    /// <summary>
    /// Results the not null.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <typeparam name="T"></typeparam>
    public static T ResultNotNull<T>(T result) where T : class
    {
        return ResultNotNull(result, "Post condition failed");
    }

    private static string GetTypeMessage(Type type)
    {
        return $"An instance of {type} was null.";
    }

    /// <summary>
    /// Formats a string.
    /// </summary>
    /// <param name="pattern">The format pattern.</param>
    /// <param name="args">The args.</param>
    private static string Format(string pattern, object[] args)
    {
        ArgumentNotNull(pattern, "pattern");
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is XmlNode xmlNode)
                {
                    args[i] = xmlNode.OuterXml;
                }
            }

            return string.Format(pattern, args);
        }

        return pattern;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Runtime.ExceptionServices;
using PowerCSharp.Extensions.Types;

namespace PowerCSharp.Extensions.Objects;

/// <summary>
/// Provides extension methods for working with exceptions.
/// Includes utilities for retrieving detailed exception messages and stack traces.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Demystifies the given exception and enhances its stack trace for better readability.
    /// </summary>
    /// <typeparam name="T">The type of the exception, which must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="exception">The exception to demystify. If <c>null</c>, the method returns <c>null</c>.</param>
    /// <returns>The demystified exception with an improved stack trace, or <c>null</c> if the input exception is <c>null</c>.</returns>
    public static T Demystify<T>(this T exception) 
        where T : Exception
    {
        if (exception == null)
        {
            return null;
        }

        return System.Diagnostics.ExceptionExtensions.Demystify(exception);
    }

    /// <summary>
    /// Retrieves a message that describes the current exception, including messages from all inner exceptions.
    /// Optionally includes the stack trace of the exception.
    /// </summary>
    /// <param name="input">The exception to retrieve messages from.</param>
    /// <param name="includeStackTrace">Set to <c>true</c> to include stack trace data in the returned messages.</param>
    /// <returns>A string containing the exception message and inner exception messages, optionally with stack trace data.</returns>
    public static string GetInnerMessages(this Exception input, bool includeStackTrace = false)
    {
        if (input == null)
        {
            return "--";
        }

        var result = input.Message;

        if (includeStackTrace)
        {
            var stackTrace = input.StackTrace?
                .Split(
                    new string[] { Environment.NewLine },
                    StringSplitOptions.None
                )
                .FirstOrDefault()
                .Trim();

            result = $"{input.Message} {stackTrace}"
                .Trim();
        }

        if (input.InnerException != null)
        {
            result = $"{result} >>>>> {input.InnerException.GetInnerMessages(includeStackTrace)}";
        }

        return result;
    }

    /// <summary>
    /// Retrieves a string containing the full stack trace of the exception, including all inner exceptions.
    /// </summary>
    /// <param name="ex">The exception to retrieve the stack trace from.</param>
    /// <returns>A string containing the full stack trace of the exception and its inner exceptions.</returns>
    public static string GetFullStackTraceString(this Exception ex)
    {
        var fullStackTraceList = ex.GetFullStackTrace();
        fullStackTraceList = fullStackTraceList.Reverse();

        var result = string.Join(
            Environment.NewLine,
            fullStackTraceList
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray()
        );

        return result;
    }

    /// <summary>
    /// Retrieves a collection of strings containing the stack traces of the exception and all inner exceptions.
    /// </summary>
    /// <param name="ex">The exception to retrieve stack traces from.</param>
    /// <returns>A collection of strings, each containing the stack trace of an exception in the chain.</returns>
    public static IEnumerable<string> GetFullStackTrace(this Exception ex)
    {
        if (ex == null)
        {
            yield break;
        }

        var innerException = ex;

        do
        {
            var exceptionMessage = $"--- Exception: {innerException.Message} ---";

            var result = new StringBuilder()
                .AppendLine(exceptionMessage)
                .AppendLine(innerException.StackTrace)
                .AppendLine("--- End of inner exception stack trace ---")
                .ToString();

            yield return result;
            innerException = innerException.InnerException;
        }
        while (innerException != null);
    }

    /// <summary>
    /// Attempts to add a key/value pair to the Exception.Data dictionary
    /// without throwing an exception if the key already exists.
    /// Complex objects are automatically serialized to JSON for safe storage.
    /// </summary>
    /// <returns>True if added; False if key already exists or if an error occurs.</returns>
    public static bool TryAddData(this Exception ex, object key, object value)
    {
        if (ex == null || key == null)
        { 
            return false; 
        }

        // Data dictionary can be null, check before access
        if (ex.Data == null)
        {
            return false;
        }

        try
        {
            // Check if key already exists
            if (ex.Data.Contains(key))
            {
                return false;
            }

            // Handle null values
            if (value == null)
            {
                ex.Data.Add(key, null);
                return true;
            }

            // Get the type of the value to determine how to handle it
            var valueType = value.GetType();

            // For primitive types and strings, add directly
            if (valueType.IsSimpleType())
            {
                ex.Data.Add(key, value);
                return true;
            }

            // For complex objects, serialize to JSON
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, new JsonSerializerOptions
                {
#if NET8_0_OR_GREATER
                    // Handle circular references by ignoring them
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
#endif
                    // Write indented for better readability in exception data
                    WriteIndented = false,
                    // Ignore read-only properties to avoid serialization issues
                    IgnoreReadOnlyProperties = true,
                    // Don't throw on errors
                    MaxDepth = 32 // Prevent stack overflow from deeply nested objects
                });

                ex.Data.Add(key, serializedValue);
                return true;
            }
            catch (JsonException)
            {
                // If serialization fails, try to add the object's string representation
                ex.Data.Add(key, value.ToString());
                return true;
            }
        }
        catch (Exception)
        {
            // If anything goes wrong, don't throw - just return false
            // This method should never cause an exception itself
            return false;
        }
    }

    /// <summary>
    /// Returns a boolean indicating whether a specified string occurs within the Message of this Exception or their InnerExceptions.
    /// </summary>
    /// <param name="input">The exception to search in.</param>
    /// <param name="value">The string to seek.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>true if the value parameter occurs within the Message of this Exception (or their InnerExceptions); otherwise, false.</returns>
    public static bool MessageContains(this Exception? input, string value, StringComparison comparisonType)
    {
        if (input == null || string.IsNullOrEmpty(value))
        {
            return false;
        }

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        if (input.Message.Contains(value, comparisonType))
#else
        if (string.Equals(input.Message, value, comparisonType) || input.Message.IndexOf(value, 0, comparisonType) >= 0)
#endif
        {
            return true;
        }
        else
        {
            return MessageContains(input.InnerException, value, comparisonType);
        }
    }

    /// <summary>
    /// Returns a collection with all InnerExceptions walking to all levels hierarchical.
    /// </summary>
    /// <param name="ex">The exception to get inner exceptions from.</param>
    /// <param name="includeItself">Set <c>true</c> to include the root Exception in the collection.</param>
    /// <returns>An enumerable collection of inner exceptions in hierarchical order.</returns>
    public static IEnumerable<Exception> GetInnerExceptions(this Exception? ex, bool includeItself = false)
    {
        if (ex == null)
        {
            yield break;
        }

        var innerException = ex.InnerException;
        if (includeItself)
        {
            innerException = ex;
        }

        if (innerException == null)
        {
            yield break;
        }

        do
        {
            yield return innerException;
            innerException = innerException.InnerException;
        }
        while (innerException != null);
    }

    /// <summary>
    /// Attempts to prepare the exception for re-throwing by preserving the stack trace. The returned exception should be immediately thrown.
    /// </summary>
    /// <param name="exception">The exception. May not be <c>null</c>.</param>
    /// <returns>The <see cref="Exception"/> that was passed into this method.</returns>
    /// <remarks>
    /// This method uses ExceptionDispatchInfo.Capture to preserve the original stack trace when re-throwing exceptions.
    /// The method should be used immediately before throwing: <code>throw exception.PrepareForRethrow();</code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the exception parameter is null.</exception>
    public static Exception PrepareForRethrow(this Exception exception)
    {
#if NET5_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(exception);
#else
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }
#endif

        ExceptionDispatchInfo
            .Capture(exception)
            .Throw();

        // The code cannot ever get here. We just return a value to work around a badly-designed API (ExceptionDispatchInfo.Throw):
        // https://connect.microsoft.com/VisualStudio/feedback/details/689516/exceptiondispatchinfo-api-modifications (http://www.webcitation.org/6XQ7RoJmO)
        return exception;
    }

    /// <summary>
    /// Adds Context information to the Exception for Diagnostics and troubleshooting
    /// </summary>
    /// <typeparam name="T">The type of the exception, which must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="input">The exception to add context to.</param>
    /// <returns>The exception with added context information.</returns>
    public static T AddContext<T>(this T input)
        where T : Exception
    {
        if (input == null)
        {
            return input;
        }

        var exception = input.Demystify();

        // TODO: Implement correlation ID management
        // TODO: Replace with proper correlation ID retrieval
        var correlationId = "TODO: Implement correlation ID retrieval";
        
        // TODO: Replace with proper context addition mechanism
        // This should add context information to the exception for diagnostics
        exception.TryAddData("CorrelationId", correlationId);

        return exception;
    }

    /// <summary>
    /// Adds Context information to the Exception for Diagnostics and troubleshooting
    /// </summary>
    /// <typeparam name="T">The type of the exception, which must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="input">The exception to add context to.</param>
    /// <param name="request">The HTTP request message to extract context from.</param>
    /// <returns>The exception with added context information.</returns>
    public static T AddContext<T>(this T input, System.Net.Http.HttpRequestMessage? request)
        where T : Exception
    {
        if (input == null)
        {
            return input;
        }

        var exception = input.Demystify();

        // TODO: Implement correlation ID extraction from HttpRequestMessage
        var correlationId = "TODO: Implement correlation ID retrieval from HttpRequestMessage";
        
        // TODO: Replace with proper context addition mechanism
        exception.TryAddData("CorrelationId", correlationId);
        exception.TryAddData("RequestUri", request?.RequestUri?.ToString());

        return exception;
    }
}

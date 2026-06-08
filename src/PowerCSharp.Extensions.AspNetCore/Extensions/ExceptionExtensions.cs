using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace PowerCSharp.Extensions.AspNetCore.Extensions;

/// <summary>
/// Provides ASP.NET Core specific extension methods for working with exceptions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Adds Context information to the Exception for Diagnostics and troubleshooting
    /// </summary>
    /// <typeparam name="T">The type of the exception, which must inherit from <see cref="Exception"/>.</typeparam>
    /// <param name="input">The exception to add context to.</param>
    /// <param name="request">The HTTP request to extract context from.</param>
    /// <returns>The exception with added context information.</returns>
    public static T AddContext<T>(this T input, HttpRequest? request)
        where T : Exception
    {
        if (input == null)
        {
            return input;
        }

        var exception = input.Demystify();

        // TODO: Implement correlation ID extraction from HttpRequest
        var correlationId = "TODO: Implement correlation ID retrieval from HttpRequest";
        
        // TODO: Replace with proper context addition mechanism
        exception.TryAddData("CorrelationId", correlationId);
        exception.TryAddData("RequestPath", request?.Path ?? string.Empty);
        exception.TryAddData("RequestQueryString", request?.QueryString.ToString() ?? string.Empty);
        exception.TryAddData("RequestMethod", request?.Method ?? string.Empty);
        exception.TryAddData("RequestScheme", request?.Scheme ?? string.Empty);
        exception.TryAddData("RequestHost", request?.Host.ToString() ?? string.Empty);

        return exception;
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
                    // Handle circular references by ignoring them
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
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
}

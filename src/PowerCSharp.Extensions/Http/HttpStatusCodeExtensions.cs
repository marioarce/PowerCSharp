using System;
using System.Net;

namespace PowerCSharp.Extensions.Http;

/// <summary>
/// Extension methods for HTTP status codes
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Checks if the HTTP status code is successful (2xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 2xx range; otherwise, false.</returns>
    public static bool IsSuccessful(this HttpStatusCode statusCode)
    {
        var intStatusCode = (int)statusCode;
        return intStatusCode.IsSuccessful();
    }

    /// <summary>
    /// Checks if the HTTP status code is successful (2xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 2xx range; otherwise, false.</returns>
    public static bool IsSuccessful(this int statusCode)
    {
        return statusCode >= 200 && statusCode <= 299;
    }

    /// <summary>
    /// Checks if the HTTP status code indicates caching (NotModified)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is NotModified; otherwise, false.</returns>
    public static bool IsCaching(this HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.NotModified;
    }

    /// <summary>
    /// Checks if the HTTP status code is a client error (4xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 4xx range; otherwise, false.</returns>
    public static bool IsClientError(this HttpStatusCode statusCode)
    {
        var intStatusCode = (int)statusCode;
        return intStatusCode.IsClientError();
    }

    /// <summary>
    /// Checks if the HTTP status code is a client error (4xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 4xx range; otherwise, false.</returns>
    public static bool IsClientError(this int statusCode)
    {
        return statusCode >= 400 && statusCode <= 499;
    }

    /// <summary>
    /// Checks if the HTTP status code is a server error (5xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 5xx range; otherwise, false.</returns>
    public static bool IsServerError(this HttpStatusCode statusCode)
    {
        var intStatusCode = (int)statusCode;
        return intStatusCode.IsServerError();
    }

    /// <summary>
    /// Checks if the HTTP status code is a server error (5xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 5xx range; otherwise, false.</returns>
    public static bool IsServerError(this int statusCode)
    {
        return statusCode >= 500 && statusCode <= 599;
    }

    /// <summary>
    /// Checks if the HTTP status code is an error (4xx or 5xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the error range; otherwise, false.</returns>
    public static bool IsError(this HttpStatusCode statusCode)
    {
        return IsClientError(statusCode) || IsServerError(statusCode);
    }

    /// <summary>
    /// Checks if the HTTP status code is an error (4xx or 5xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the error range; otherwise, false.</returns>
    public static bool IsError(this int statusCode)
    {
        return IsClientError(statusCode) || IsServerError(statusCode);
    }

    /// <summary>
    /// Checks if the HTTP status code is in the redirect category (3xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 3xx range; otherwise, false.</returns>
    public static bool IsRedirect(this HttpStatusCode statusCode)
    {
        var intStatusCode = (int)statusCode;
        return intStatusCode.IsRedirect();
    }

    /// <summary>
    /// Checks if the HTTP status code is in the redirect category (3xx range)
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>True if the status code is in the 3xx range; otherwise, false.</returns>
    public static bool IsRedirect(this int statusCode)
    {
        return statusCode >= 300 && statusCode <= 399;
    }
}

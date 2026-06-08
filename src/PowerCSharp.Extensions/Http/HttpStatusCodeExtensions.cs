using System;
using System.Net;

namespace PowerCSharp.Extensions.Http;

/// <summary>
/// Extension methods for HTTP status codes
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Checks if the HTTP status code is successful (2xx range).
    /// This method provides semantic understanding of HTTP response status for proper error handling.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check.</param>
    /// <returns>
    /// True if the status code is in the 2xx range (200-299); otherwise, false.
    /// This includes all standard success codes: 200 OK, 201 Created, 202 Accepted, 204 No Content, etc.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Status Codes Considered Successful:</strong>
    /// - 200 OK: Request succeeded
    /// - 201 Created: Resource created successfully
    /// - 202 Accepted: Request accepted for processing
    /// - 204 No Content: Request succeeded with no response body
    /// - And all other 2xx status codes
    /// </para>
    /// <para>
    /// <strong>Edge Cases:</strong>
    /// - Custom status codes in 2xx range: Returns true
    /// - Non-standard status codes: Evaluated based on numeric range
    /// - Status code 0: Returns false (not a valid HTTP status)
    /// </para>
    /// <para>
    /// <strong>Security Considerations:</strong>
    /// - Proper status code classification prevents security misconfigurations
    /// - Accurate success/failure detection for logging and monitoring
    /// - Helps prevent information leakage through incorrect status handling
    /// </para>
    /// <strong>Examples:</strong>
    /// <code>
    /// HttpStatusCode.OK.IsSuccessful(); // true
    /// HttpStatusCode.Created.IsSuccessful(); // true
    /// HttpStatusCode.NotFound.IsSuccessful(); // false
    /// HttpStatusCode.InternalServerError.IsSuccessful(); // false
    /// 
    /// // In HTTP client code
    /// var response = await httpClient.GetAsync(url);
    /// if (response.StatusCode.IsSuccessful())
    /// {
    ///     var content = await response.Content.ReadAsStringAsync();
    ///     return content;
    /// }
    /// else
    /// {
    ///     throw new HttpRequestException($"Request failed with status {response.StatusCode}");
    /// }
    /// </code>
    /// </remarks>
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

using System.Web;

namespace PowerCSharp.Compatibility.Http;

/// <summary>
/// Extension methods for HttpRequest operations, primarily focused on client IP address extraction.
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Gets the User's client IP Address from an HttpRequest.
    /// Wraps the HttpRequestBase functionality for newer ASP.NET applications.
    /// </summary>
    /// <param name="request">The HttpRequest to extract IP from.</param>
    /// <returns>An IP Address; null if error or not found.</returns>
    public static string? GetClientIpAddress(this HttpRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        var httpRequestBase = new HttpRequestWrapper(request);
        return httpRequestBase?.GetClientIpAddress();
    }
}

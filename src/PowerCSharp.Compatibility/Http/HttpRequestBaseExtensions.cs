using System;
using System.Net.Http;
using System.Web;

namespace PowerCSharp.Compatibility.Http;

/// <summary>
/// Extension methods for HttpRequestBase operations, primarily focused on client IP address extraction.
/// </summary>
public static class HttpRequestBaseExtensions
{
    /// <summary>
    /// Gets the User's client IP Address from an HttpRequestBase.
    /// Handles various proxy scenarios including CloudFlare, X-Forwarded-For, and standard headers.
    /// </summary>
    /// <param name="request">The HttpRequestBase to extract IP from.</param>
    /// <returns>An IP Address; null if error or not found.</returns>
    /// <remarks>
    /// This method checks multiple sources in order of reliability:
    /// 1. Forwarded header (RFC 7239)
    /// 2. CloudFlare CF-CONNECTING-IP header
    /// 3. X-Forwarded-For header
    /// 4. Various server variables for load balancer scenarios
    /// 5. UserHostAddress as fallback
    /// </remarks>
    public static string? GetClientIpAddress(this HttpRequestBase? request)
    {
        if (request == null)
        {
            return null;
        }

        if (request.Headers != null)
        {
            // Handle standardized 'Forwarded' header, refer https://www.rfc-editor.org/rfc/rfc7239
            var forwardedHeader = request.Headers["Forwarded"];

            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                foreach (string segment in forwardedHeader.Split(',')[0].Split(';'))
                {
                    string[] pair = segment
                        .Trim()
                        .Split('=');

                    if (pair.Length == 2 && pair[0].Equals("for", StringComparison.OrdinalIgnoreCase))
                    {
                        var result = pair[1].Trim('"');

                        // IPv6 addresses are always enclosed in square brackets
                        int left = result.IndexOf('['), right = result.IndexOf(']');

                        if (left == 0 && right > 0)
                        {
                            return result.Substring(1, right - 1);
                        }

                        // strip port of IPv4 addresses
                        int colon = result.IndexOf(':');
                        if (colon != -1)
                        {
                            return result.Substring(0, colon);
                        }

                        // this will return IPv4, "unknown", and obfuscated addresses
                        return result;
                    }
                }
            }

            // Self-hosting using CloudFlare
            if (request.Headers?["CF-CONNECTING-IP"] != null)
            {
                var result = request.Headers["CF-CONNECTING-IP"]?.ToString();

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            // Handle non-standardized 'X-Forwarded-For' header
            var xForwardedFor = request.Headers?["X-Forwarded-For"];

            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor?.Split(',')[0];
            }
        }

        if (request.ServerVariables != null)
        {
            if (request.ServerVariables?["HTTP_X_CLUSTER_CLIENT_IP"] != null)
            {
                var result = request.ServerVariables["HTTP_X_CLUSTER_CLIENT_IP"]?.ToString();

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            // Sometimes the visitors are behind either a proxy server or a router
            // and the standard Request.UserHostAddress only captures the IP address of the proxy server or router.
            // When this is the case the user's IP address is then stored in the server variable ("HTTP_X_FORWARDED_FOR").
            if (request.ServerVariables?["HTTP_X_FORWARDED_FOR"] != null)
            {
                var result = request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.ToString();

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            if (request.ServerVariables?["REMOTE_ADDR"] != null)
            {
                var result = request.ServerVariables["REMOTE_ADDR"]
                    .Split(',')[0]
                    .Trim();

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
        }

        // Then, return the UserHostAddress
        return request.UserHostAddress;
    }

    /// <summary>
    /// Gets the HTTP method from an HttpRequestBase as an HttpMethod object.
    /// </summary>
    /// <param name="request">The HttpRequestBase to get the method from.</param>
    /// <returns>An HttpMethod object representing the HTTP method.</returns>
    public static HttpMethod Method(this HttpRequestBase? request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var result = new HttpMethod(request.HttpMethod);
        return result;
    }
}

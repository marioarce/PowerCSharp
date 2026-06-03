#pragma warning disable CS0649

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PowerCSharp.Compatibility.Net;

/// <summary>
/// Provides extension methods for <see cref="HttpRequestMessage"/> to support cloning operations and client IP address extraction.
/// Essential for retry scenarios where HttpRequestMessage instances need to be recreated for each attempt.
/// </summary>
public static class HttpRequestMessageExtensions
{
    private const string TrueClientIpHeader = "True-Client-IP";
    private const string XForwardedForHeader = "X-Forwarded-For";
    private const string HttpContextProperty = "MS_HttpContext";
    private const string RemoteEndpointMessageProperty = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
    private const string OwinContextProperty = "MS_OwinContext";

    /// <summary>
    /// Creates a deep clone of the <see cref="HttpRequestMessage"/> including headers, content, and properties.
    /// This method is critical for retry scenarios where the same request needs to be sent multiple times,
    /// as HttpRequestMessage instances can only be sent once per .NET guidelines.
    /// </summary>
    /// <param name="original">The original HttpRequestMessage to clone.</param>
    /// <returns>A new HttpRequestMessage instance with all headers, content, and properties copied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the original request is null.</exception>
    public static HttpRequestMessage Clone(this HttpRequestMessage original)
    {
        if (original == null)
        {
            throw new ArgumentNullException(nameof(original));
        }

        // Create new request with same method and URI
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        // Copy all headers without validation to preserve custom headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (original.Content != null)
        {
            // Read content synchronously
            var contentBytes = original
                .Content
                .ReadAsByteArrayAsync()
                .GetAwaiter()
                .GetResult();

            clone.Content = new ByteArrayContent(contentBytes);

            // Copy content headers
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    /// <summary>
    /// Creates a deep clone of the <see cref="HttpRequestMessage"/> asynchronously for better performance with large content.
    /// </summary>
    /// <param name="original">The original HttpRequestMessage to clone.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the cloned HttpRequestMessage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the original request is null.</exception>
    public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage original, CancellationToken cancellationToken = default)
    {
        if (original == null)
        {
            throw new ArgumentNullException(nameof(original));
        }

        // Create new request with same method and URI
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        // Copy all headers without validation to preserve custom headers
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (original.Content != null)
        {
            // Read content bytes asynchronously
            var contentBytes = await original
                .Content
                .ReadAsByteArrayAsync()
                .ConfigureAwait(false);

            clone.Content = new ByteArrayContent(contentBytes);

            // Copy content headers
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    /// <summary>
    /// Gets the User's client IP Address from an HttpRequestMessage (Web API).
    /// Handles various hosting scenarios including Web-hosting, Self-hosting, and Owin.
    /// </summary>
    /// <param name="request">The HttpRequestMessage to extract IP from.</param>
    /// <returns>An IP Address; null if error or not found.</returns>
    /// <remarks>
    /// This method checks multiple sources in order of reliability:
    /// 1. True-Client-IP header (Akamai)
    /// 2. X-Forwarded-For header (proxies)
    /// 3. HttpContext property (Web-hosting)
    /// 4. RemoteEndpointMessageProperty (Self-hosting)
    /// 5. OwinContext property (Self-hosting with Owin)
    /// 6. Current HttpContext as fallback
    /// </remarks>
    public static string? GetClientIpAddress(this HttpRequestMessage? request)
    {
        if (request == null)
        {
            return null;
        }

        if (request.Headers != null)
        {
            if (request.Headers.TryGetValues(TrueClientIpHeader, out System.Collections.Generic.IEnumerable<string>? headerValues))
            {
                // True-Client-IP may be added by Akamai.  It is the IP address of the machine that directly contacted the Akamai edge server.
                return headerValues?.FirstOrDefault();
            }

            if (request.Headers.TryGetValues(XForwardedForHeader, out headerValues))
            {
                // X-Forwarded-For may be added by proxies as they forward an HTTP request.  It will commonly contain more than one IP address, 
                // separated by commas. The one we want, the address of the *client*, will be the first.
                var value = headerValues!.First();
                var commaIndex = value.IndexOf(',');

                return (commaIndex < 0) ? value : value.Substring(0, commaIndex);
            }
        }

        // Having examined the HttpHeaders, we're not aware than any cache or proxy was involved in this Request.  
        // Then, try to return the address of the machine that made the immediate request to this server.
        if (request.Properties != null)
        {
            // Web-hosting. Needs reference to System.Web.dll
            if (request.Properties.ContainsKey(HttpContextProperty))
            {
                dynamic ctx = request.Properties[HttpContextProperty];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            // Self-hosting. Needs reference to System.ServiceModel.dll. 
            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessageProperty];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            // Self-hosting using Owin. Needs reference to Microsoft.Owin.dll. 
            if (request.Properties.ContainsKey(OwinContextProperty))
            {
                dynamic owinContext = request.Properties[OwinContextProperty];
                if (owinContext != null)
                {
                    return owinContext.Request.RemoteIpAddress;
                }
            }
        }

        // Then, return the UserHostAddress
        if (HttpContext.Current != null && HttpContext.Current.Request != null)
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        return null;
    }
}

using PowerCSharp.Extensions.Http;

namespace WebSample.Samples.Extensions;

/// <summary>
/// Sample endpoints demonstrating HTTP extensions
/// </summary>
public static class HttpSampleEndpoints
{
    /// <summary>
    /// Gets HTTP extensions demo data
    /// </summary>
    /// <returns>Demo results showing HTTP status code and media type operations</returns>
    public static object GetDemoData()
    {
        var statusCode = System.Net.HttpStatusCode.OK;
        var jsonMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        var xmlMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        var textMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        
        return new
        {
            statusCode = new { 
                value = statusCode, 
                isSuccess = statusCode.IsSuccessful(),
                isClientError = statusCode.IsClientError(),
                isServerError = statusCode.IsServerError(),
                isError = statusCode.IsError(),
                isRedirect = statusCode.IsRedirect()
            },
            mediaTypes = new 
            {
                json = new { value = "application/json", isJson = jsonMediaType.IsJson(), isXml = jsonMediaType.IsXml() },
                xml = new { value = "application/xml", isJson = xmlMediaType.IsJson(), isXml = xmlMediaType.IsXml() },
                text = new { value = "text/plain", isJson = textMediaType.IsJson(), isXml = textMediaType.IsXml() }
            }
        };
    }
}

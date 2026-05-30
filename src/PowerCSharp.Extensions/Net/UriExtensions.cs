using System;
using Microsoft.AspNetCore.WebUtilities;

namespace PowerCSharp.Extensions.Net;

/// <summary>
/// Extension methods for URI operations
/// </summary>
public static class UriExtensions
{
    /// <summary>
    /// Adds the specified parameter to the query string of the URI.
    /// </summary>
    /// <param name="url">The base URI to add the parameter to.</param>
    /// <param name="parameterName">The name of the parameter to add.</param>
    /// <param name="parameterValue">The value for the parameter to add.</param>
    /// <returns>A new URI with the added parameter in the query string.</returns>
    public static Uri AddParameter(this Uri url, string parameterName, string parameterValue)
    {
        var uriBuilder = new UriBuilder(url);
        uriBuilder.Query = QueryHelpers.AddQueryString(uriBuilder.Query, parameterName, parameterValue);

        return uriBuilder.Uri;
    }
}

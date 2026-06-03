using System;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace PowerCSharp.Extensions.AspNetCore.Extensions;

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

    /// <summary>
    /// Gets the scheme and authority segments of the URI.
    /// </summary>
    /// <param name="uri">The URI to extract authority from.</param>
    /// <returns>A string with the Schema + Host Name + Port Number (if applies), or empty string if URI is null.</returns>
    public static string GetAuthority(this Uri? uri)
    {
        return uri?.GetLeftPart(UriPartial.Authority) ?? string.Empty;
    }

    /// <summary>
    /// Appends one or more paths to the end of the URI.
    /// </summary>
    /// <param name="uri">Base URI to which paths will be appended.</param>
    /// <param name="paths">One or more strings representing paths to append to URI.</param>
    /// <returns>A new Uri instance representing the result of appending the paths to the URI.</returns>
    /// <example>
    /// <code>
    /// var baseUri = new Uri("https://example.com/api");
    /// var resultUri = baseUri.Append("users", "1");
    /// // Result: https://example.com/api/users/1
    /// </code>
    /// </example>
    public static Uri Append(this Uri uri, params string[] paths)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (paths == null || paths.Length == 0)
        {
            return uri;
        }

        var uriString = paths.Aggregate(
            uri.AbsoluteUri, 
            (current, path) => $"{current.TrimEnd('/')}/{path.TrimStart('/')}"
        );

        return new Uri(uriString);
    }
}

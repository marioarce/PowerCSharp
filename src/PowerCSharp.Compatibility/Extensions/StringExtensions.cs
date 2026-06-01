using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace PowerCSharp.Compatibility.Extensions;

/// <summary>
/// Provides extension methods for strings with System.Web dependencies.
/// Includes utilities for URL query parameter manipulation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Appends query parameters to the specified URL.
    /// </summary>
    /// <param name="url">The original URL.</param>
    /// <param name="parameters">A dictionary of query parameters to append.</param>
    /// <returns>The URL with the parameters appended.</returns>
    public static string AppendQueryParameters(this string url, IDictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        if (parameters == null || parameters.Count == 0)
        {
            return url;
        }

        var query = HttpUtility.ParseQueryString(url);
        foreach (var parameter in parameters)
        {
            query.Add(parameter.Key, parameter.Value);
        }

        var outputUrl = HttpUtility.UrlDecode(query.ToString());
        var rex = new Regex("&", RegexOptions.IgnoreCase);

        return outputUrl.IndexOf('?') == -1 ? rex.Replace(outputUrl, "?", 1) : outputUrl;
    }
}

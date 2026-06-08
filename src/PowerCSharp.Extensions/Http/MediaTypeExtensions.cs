using System;
using System.Net.Http.Headers;

namespace PowerCSharp.Extensions.Http;

/// <summary>
/// Extension methods for MediaTypeHeaderValue operations.
/// </summary>
public static class MediaTypeExtensions
{
    /// <summary>
    /// Returns true if the MediaTypeHeaderValue given is XML.
    /// </summary>
    /// <param name="input">The media type header to check.</param>
    /// <returns>true if the MediaType is XML; otherwise false.</returns>
    public static bool IsXml(this MediaTypeHeaderValue? input)
    {
        if (input == null)
        {
            return false;
        }

        var responseMediaType = input.MediaType;
        return responseMediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase)
            ||
            responseMediaType.Equals("text/xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true if the MediaTypeHeaderValue given is JSON.
    /// </summary>
    /// <param name="input">The media type header to check.</param>
    /// <returns>true if the MediaType is JSON; otherwise false.</returns>
    public static bool IsJson(this MediaTypeHeaderValue? input)
    {
        if (input == null)
        {
            return false;
        }

        var responseMediaType = input.MediaType;
        return responseMediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
            ||
            responseMediaType.Equals("text/json", StringComparison.OrdinalIgnoreCase);
    }
}

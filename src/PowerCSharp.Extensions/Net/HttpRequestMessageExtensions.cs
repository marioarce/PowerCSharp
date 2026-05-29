using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PowerCSharp.Extensions.Net;

/// <summary>
/// Provides extension methods for <see cref="HttpRequestMessage"/> to support cloning operations.
/// Essential for retry scenarios where HttpRequestMessage instances need to be recreated for each attempt.
/// </summary>
public static class HttpRequestMessageExtensions
{
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
            throw new ArgumentNullException(nameof(original));

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
            var contentBytes = original.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
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
            var contentBytes = await original.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            clone.Content = new ByteArrayContent(contentBytes);

            // Copy content headers
            foreach (var header in original.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}

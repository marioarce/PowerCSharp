using System;
using System.IO;
using System.Threading.Tasks;

namespace PowerCSharp.Extensions.Streams;

/// <summary>
/// Extensions for <see cref="Stream"/>
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Asynchronously clones the content of this stream to another stream.
    /// </summary>
    /// <param name="stream">The source stream to clone.</param>
    /// <param name="destination">The destination stream. If null, a new MemoryStream will be created.</param>
    /// <returns>A task that represents the asynchronous clone operation.</returns>
    public static async Task CloneAsync(this Stream stream, Stream? destination)
    {
        if (stream == null)
        {
            return;
        }

        destination ??= new MemoryStream();

        await stream.CopyToAsync(destination)
            .ConfigureAwait(false);
        destination.Position = 0;
    }
}

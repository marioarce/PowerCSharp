using System.Collections.Generic;
using System.IO;

namespace PowerCSharp.Utilities;

/// <summary>
/// Utility class for common stream operations
/// </summary>
public static class StreamUtility
{
    /// <summary>
    /// Disposes multiple streams safely.
    /// </summary>
    /// <param name="streams">Collection of streams to dispose</param>
    public static void DisposeStreams(IEnumerable<Stream?>? streams)
    {
        if (streams == null)
        {
            return;
        }

        foreach (var stream in streams)
        {
            try
            {
                stream?.Dispose();
            }
            catch { /* swallow */ }
        }
    }
}

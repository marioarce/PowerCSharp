using System;

namespace PowerCSharp.Compatibility.Helpers;

/// <summary>
/// Provides helper methods for timestamp operations
/// </summary>
public class TimestampHelper
{
    /// <summary>
    /// Gets the total number of milliseconds that have elapsed since the Unix epoch.
    /// The Unix epoch is the time 00:00:00 UTC on 1st January 1970.
    /// </summary>
    public static long GetTimestamp()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long result = now.ToUnixTimeMilliseconds();
        return result;
    }
}

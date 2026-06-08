using PowerCSharp.Extensions;

namespace WebSample.Samples.Utilities;

/// <summary>
/// Sample endpoints demonstrating Unix timestamp utilities
/// </summary>
public static class UnixTimestampSampleEndpoints
{
    /// <summary>
    /// Gets Unix timestamp demo data
    /// </summary>
    /// <returns>Demo results showing Unix timestamp operations</returns>
    public static object GetDemoData()
    {
        var now = DateTime.Now;
        var specificDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        return new
        {
            currentDate = new { 
                date = now.ToString("yyyy-MM-dd HH:mm:ss"), 
                unixTimestamp = now.GetUnixTimestamp() 
            },
            specificDate = new { 
                date = specificDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                unixTimestamp = specificDate.GetUnixTimestamp() 
            }
        };
    }
}

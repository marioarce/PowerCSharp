using PowerCSharp.Helpers;

namespace WebSample.Samples.Helpers;

/// <summary>
/// Sample endpoints demonstrating async helper utilities
/// </summary>
public static class AsyncSampleEndpoints
{
    /// <summary>
    /// Gets async helper demo data
    /// </summary>
    /// <returns>Demo results showing async-to-sync bridging</returns>
    public static object GetDemoData()
    {
        // Simulate async operation being called from sync context
        string result = AsyncHelper.RunSync(async () =>
        {
            await Task.Delay(100); // Simulate async work
            return "Async operation completed successfully!";
        });
        
        return new
        {
            message = result,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}

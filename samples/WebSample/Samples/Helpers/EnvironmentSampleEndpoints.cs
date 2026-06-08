using PowerCSharp.Extensions.AspNetCore.Helpers;

namespace WebSample.Samples.Helpers;

/// <summary>
/// Sample endpoints demonstrating environment helper utilities
/// </summary>
public static class EnvironmentSampleEndpoints
{
    /// <summary>
    /// Gets environment demo data
    /// </summary>
    /// <returns>Demo results showing environment information</returns>
    public static object GetDemoData()
    {
        return new
        {
            machineName = EnvironmentHelper.GetSafeMachineName(),
            isDevelopment = EnvironmentHelper.IsDevelopment(),
            appVersion = EnvironmentHelper.GetApplicationVersion(),
            dotnetEnvironment = EnvironmentHelper.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Not set")
        };
    }
}

using PowerCSharp.Extensions;

namespace WebSample.Samples.Extensions;

/// <summary>
/// Sample endpoints demonstrating DateTime extensions
/// </summary>
public static class DateTimeSampleEndpoints
{
    /// <summary>
    /// Gets DateTime extensions demo data
    /// </summary>
    /// <returns>Demo results showing DateTime operations</returns>
    public static object GetDemoData()
    {
        var today = DateTime.Now;
        var birthDate = new DateTime(1990, 1, 1);
        
        return new
        {
            birthDate = birthDate,
            today = today.ToString("yyyy-MM-dd"),
            age = birthDate.GetAge(),
            isWeekend = today.IsWeekend(),
            firstDayOfMonth = today.FirstDayOfMonth().ToString("yyyy-MM-dd"),
            lastDayOfMonth = today.LastDayOfMonth().ToString("yyyy-MM-dd")
        };
    }
}

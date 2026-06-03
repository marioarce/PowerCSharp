using PowerCSharp.Utilities;

namespace WebSample.Samples.Utilities;

/// <summary>
/// Sample endpoints demonstrating mathematical utilities
/// </summary>
public static class MathSampleEndpoints
{
    /// <summary>
    /// Gets math utilities demo data
    /// </summary>
    /// <returns>Demo results showing mathematical operations</returns>
    public static object GetDemoData()
    {
        int value = 15;
        int min = 10;
        int max = 20;
        
        return new
        {
            clamp = MathUtility.Clamp(value, min, max),
            isInRange = MathUtility.IsInRange(value, min, max),
            percentage = MathUtility.Percentage(25, 100),
            radians = MathUtility.ToRadians(180),
            degrees = MathUtility.ToDegrees(Math.PI),
            isEven = MathUtility.IsEven(4),
            isOdd = MathUtility.IsOdd(3)
        };
    }
}

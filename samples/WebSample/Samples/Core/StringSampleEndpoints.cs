namespace WebSample.Samples.Core;

/// <summary>
/// Sample endpoints demonstrating string operations and utilities
/// </summary>
public static class StringSampleEndpoints
{
    /// <summary>
    /// Gets string manipulation demo data
    /// </summary>
    /// <returns>Demo results showing various string operations</returns>
    public static object GetDemoData()
    {
        string text = "hello world from powercsharp";
        string emptyString = "";
        
        return new
        {
            original = text,
            upperCase = text.ToUpper(),
            lowerCase = text.ToLower(),
            substring = text.Substring(0, 5),
            length = text.Length,
            isNullOrEmpty = string.IsNullOrEmpty(emptyString),
            isNullOrWhiteSpace = string.IsNullOrWhiteSpace(emptyString),
            contains = text.Contains("powercsharp"),
            trim = text.Trim()
        };
    }
}

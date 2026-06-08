using PowerCSharp.Helpers;

namespace WebSample.Samples.Core;

/// <summary>
/// Sample endpoints demonstrating JSON helper utilities
/// </summary>
public static class JsonSampleEndpoints
{
    /// <summary>
    /// Gets JSON manipulation demo data
    /// </summary>
    /// <returns>Demo results showing JSON serialization and pretty printing</returns>
    public static object GetDemoData()
    {
        var person = new { Name = "Mario Arce", Age = 30, Country = "Costa Rica", Skills = new[] { "C#", ".NET", "Web Development" } };
        string json = JsonHelper.SafeSerialize(person);
        var prettyJson = JsonHelper.PrettyPrint(json);
        
        return new
        {
            serialized = json,
            prettyPrinted = prettyJson
        };
    }
}

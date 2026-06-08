using PowerCSharp.Core.Collections;

namespace WebSample.Samples.Utilities;

/// <summary>
/// Sample endpoints demonstrating dictionary extension utilities
/// </summary>
public static class DictionarySampleEndpoints
{
    /// <summary>
    /// Gets dictionary extensions demo data
    /// </summary>
    /// <returns>Demo results showing dictionary operations</returns>
    public static object GetDemoData()
    {
        var dict = new Dictionary<string, int>
        {
            { "apple", 5 },
            { "banana", 3 },
            { "orange", 8 }
        };
        
        // Test AddOrUpdate
        dict.AddOrUpdate("grape", 12); // Add new
        dict.AddOrUpdate("apple", 7);  // Update existing
        
        // Test TryAdd
        bool addedNew = dict.TryAdd("pear", 4);     // Should succeed
        bool triedExisting = dict.TryAdd("apple", 10); // Should fail
        
        return new
        {
            dictionary = dict,
            addNewKey = new { key = "grape", added = true, value = 12 },
            updateExistingKey = new { key = "apple", updated = true, newValue = 7 },
            tryAddNew = new { key = "pear", added = addedNew, value = 4 },
            tryAddExisting = new { key = "apple", added = triedExisting, attemptedValue = 10 }
        };
    }
}

using PowerCSharp.Extensions.Collections;

namespace WebSample.Samples.Utilities;

/// <summary>
/// Sample endpoints demonstrating collection extension utilities
/// </summary>
public static class CollectionSampleEndpoints
{
    /// <summary>
    /// Gets collection extensions demo data
    /// </summary>
    /// <returns>Demo results showing collection operations</returns>
    public static object GetDemoData()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        return new
        {
            numbers = numbers,
            isNullOrEmpty = numbers.IsNullOrEmpty(),
            firstOrDefaultSafe = numbers.FirstOrDefaultSafe(-1),
            page1 = numbers.Page(1, 3),
            page2 = numbers.Page(2, 3),
            filtered = numbers.Filter(x => x > 5),
            ordered = numbers.Order(x => x, descending: true)
        };
    }
}

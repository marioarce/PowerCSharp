using PowerCSharp.Extensions;
using PowerCSharp.Extensions.Objects;

namespace WebSample.Samples.Extensions;

/// <summary>
/// Sample endpoints demonstrating Hash extensions
/// </summary>
public static class HashSampleEndpoints
{
    /// <summary>
    /// Gets Hash extensions demo data
    /// </summary>
    /// <returns>Demo results showing Hash operations</returns>
    public static object GetDemoData()
    {
        var simpleObj = new { Name = "John Doe", Age = 30, Email = "john@example.com" };
        var complexObj = new 
        { 
            Id = 123, 
            Customer = new { Name = "Alice Smith", Email = "alice@example.com" }, 
            Items = new[] { new { Name = "Product1", Price = 29.99 }, new { Name = "Product2", Price = 49.99 } },
            CreatedDate = DateTime.Now,
            Tags = new[] { "electronics", "gadgets", "tech" }
        };
        
        return new
        {
            simpleObject = new
            {
                data = simpleObj,
                hash = simpleObj.ComputeHash(),
                hashLength = simpleObj.ComputeHash().Length
            },
            complexObject = new
            {
                data = complexObj,
                hash = complexObj.ComputeHash(),
                hashLength = complexObj.ComputeHash().Length
            },
            nullObject = new
            {
                data = (object?)null,
                hash = ((object?)null)!.ComputeHash()
            },
            consistencyCheck = new
            {
                hash1 = simpleObj.ComputeHash(),
                hash2 = simpleObj.ComputeHash(),
                areEqual = simpleObj.ComputeHash() == simpleObj.ComputeHash()
            },
            primitiveTypes = new
            {
                stringHash = ((object)"Hello World").ComputeHash(),
                intHash = ((object)42).ComputeHash(),
                boolHash = ((object)true).ComputeHash(),
                doubleHash = ((object)3.14159).ComputeHash()
            },
            collections = new
            {
                listHash = ((object)new List<string> { "item1", "item2", "item3" }).ComputeHash(),
                arrayHash = ((object)new[] { 1, 2, 3, 4, 5 }).ComputeHash(),
                dictHash = ((object)new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } }).ComputeHash()
            },
            performance = new
            {
                description = "Hash computation is optimized for performance with minimal allocations",
                algorithm = "SHA256 with JSON serialization",
                outputFormat = "16-character hexadecimal string",
                useCases = new[] { "Caching", "Data integrity verification", "Object identification", "Change detection" }
            }
        };
    }
}

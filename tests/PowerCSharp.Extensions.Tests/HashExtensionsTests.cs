using PowerCSharp.Extensions;
using PowerCSharp.Extensions.Objects;
using System.Text.Json;
using Xunit;

namespace PowerCSharp.Extensions.Tests;

public class HashExtensionsTests
{
    [Fact]
    public void ComputeHash_WithNullObject_ShouldReturnNull()
    {
        // Arrange
        object? obj = null;
        
        // Act
        string result = obj.ComputeHash();
        
        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void ComputeHash_WithSimpleObject_ShouldReturnConsistentHash()
    {
        // Arrange
        var obj = new { Name = "John", Age = 30 };
        
        // Act
        string hash1 = obj.ComputeHash();
        string hash2 = obj.ComputeHash();
        
        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(16, hash1.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash1);
    }

    [Fact]
    public void ComputeHash_WithComplexObject_ShouldReturnConsistentHash()
    {
        // Arrange
        var obj = new 
        { 
            Id = 123, 
            Customer = new { Name = "Alice", Email = "alice@example.com" }, 
            Items = new[] { new { Name = "Product1", Price = 29.99 } },
            CreatedDate = DateTime.Now,
            Tags = new[] { "electronics", "gadgets" }
        };
        
        // Act
        string hash1 = obj.ComputeHash();
        string hash2 = obj.ComputeHash();
        
        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(16, hash1.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash1);
    }

    [Fact]
    public void ComputeHash_WithDifferentObjects_ShouldReturnDifferentHashes()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 30 };
        var obj2 = new { Name = "Jane", Age = 25 };
        
        // Act
        string hash1 = obj1.ComputeHash();
        string hash2 = obj2.ComputeHash();
        
        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_WithSameContentDifferentReferences_ShouldReturnSameHash()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 30 };
        var obj2 = new { Name = "John", Age = 30 };
        
        // Act
        string hash1 = obj1.ComputeHash();
        string hash2 = obj2.ComputeHash();
        
        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_WithPrimitiveTypes_ShouldReturnValidHash()
    {
        // Arrange & Act & Assert
        Assert.Equal(16, ((object)"test").ComputeHash().Length);
        Assert.Equal(16, ((object)123).ComputeHash().Length);
        Assert.Equal(16, ((object)true).ComputeHash().Length);
        Assert.Equal(16, ((object)3.14).ComputeHash().Length);
    }

    [Fact]
    public void ComputeHash_WithCollections_ShouldReturnValidHash()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };
        var array = new[] { 1, 2, 3, 4, 5 };
        var dictionary = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };
        
        // Act
        string listHash = ((object)list).ComputeHash();
        string arrayHash = ((object)array).ComputeHash();
        string dictHash = ((object)dictionary).ComputeHash();
        
        // Assert
        Assert.Equal(16, listHash.Length);
        Assert.Equal(16, arrayHash.Length);
        Assert.Equal(16, dictHash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", listHash);
        Assert.Matches(@"^[A-F0-9]{16}$", arrayHash);
        Assert.Matches(@"^[A-F0-9]{16}$", dictHash);
    }

    [Fact]
    public void ComputeHash_WithDateTime_ShouldReturnValidHash()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 12, 30, 45);
        
        // Act
        string hash = ((object)dateTime).ComputeHash();
        
        // Assert
        Assert.Equal(16, hash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash);
    }

    [Fact]
    public void ComputeHash_WithNestedObjects_ShouldReturnValidHash()
    {
        // Arrange
        var nested = new 
        {
            Level1 = new 
            {
                Level2 = new 
                {
                    Level3 = new 
                    {
                        Data = "deeply nested",
                        Value = 42
                    }
                }
            }
        };
        
        // Act
        string hash = ((object)nested).ComputeHash();
        
        // Assert
        Assert.Equal(16, hash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash);
    }

    [Fact]
    public void ComputeHash_WithSelfReferencingObject_ShouldHandleGracefully()
    {
        // Arrange
        var obj = new SelfReferencingClass();
        obj.Name = "Test";
        obj.SelfReference = obj;
        
        // Act & Assert - Should not throw and should return a valid hash
        string hash = ((object)obj).ComputeHash();
        Assert.Equal(16, hash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash);
    }

    [Fact]
    public void ComputeHash_WithNonSerializableObject_ShouldReturnFallbackHash()
    {
        // Arrange
        var obj = new NonSerializableObject();
        
        // Act
        string hash = ((object)obj).ComputeHash();
        
        // Assert
        Assert.Equal(16, hash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash);
    }

    [Fact]
    public void ComputeHash_WithEmptyString_ShouldReturnValidHash()
    {
        // Arrange
        string empty = "";
        
        // Act
        string hash = ((object)empty).ComputeHash();
        
        // Assert
        Assert.Equal(16, hash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash);
    }

    [Fact]
    public void ComputeHash_WithLargeObject_ShouldReturnValidHash()
    {
        // Arrange
        var largeObj = new 
        {
            LargeString = new string('x', 10000),
            LargeArray = Enumerable.Range(0, 1000).ToArray(),
            LargeList = Enumerable.Range(0, 1000).ToList(),
            NestedData = new 
            {
                Items = Enumerable.Range(0, 100).Select(i => new 
                {
                    Id = i,
                    Name = $"Item{i}",
                    Description = new string('a', 100)
                }).ToArray()
            }
        };
        
        // Act
        string hash = ((object)largeObj).ComputeHash();
        
        // Assert
        Assert.Equal(16, hash.Length);
        Assert.Matches(@"^[A-F0-9]{16}$", hash);
    }

    [Fact]
    public void ComputeHash_PerformanceTest_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var obj = new { Name = "Performance Test", Value = 42 };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Act
        for (int i = 0; i < 1000; i++)
        {
            ((object)obj).ComputeHash();
        }
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Hash computation took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void ComputeHash_WithSameObjectModified_ShouldReturnDifferentHash()
    {
        // Arrange
        var obj = new { Name = "John", Age = 30 };
        string originalHash = obj.ComputeHash();
        
        // Simulate object modification by creating a new object with different data
        var modifiedObj = new { Name = "John", Age = 31 };
        
        // Act
        string modifiedHash = ((object)modifiedObj).ComputeHash();
        
        // Assert
        Assert.NotEqual(originalHash, modifiedHash);
    }

    // Helper classes for testing
    private class SelfReferencingClass
    {
        public string Name { get; set; } = string.Empty;
        public SelfReferencingClass? SelfReference { get; set; }
        public object? UnserializableField { get; set; }
    }

    private class NonSerializableObject
    {
        // This class has properties that might cause serialization issues
        public Action? ActionProperty { get; set; }
        public Func<int>? FuncProperty { get; set; }
        public EventHandler? EventProperty { get; set; }
        
        public string Name => "NonSerializable";
    }
}

using PowerCSharp.Helpers;
using System.Text.Json;
using Xunit;

namespace PowerCSharp.Helpers.Tests;

public class JsonHelperTests
{
    private class TestObject
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public void SafeSerialize_ShouldSerializeValidObject()
    {
        // Arrange
        var obj = new TestObject { Name = "John", Age = 30, IsActive = true };
        
        // Act
        string result = JsonHelper.SafeSerialize(obj);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("John", result);
        Assert.Contains("30", result);
        Assert.Contains("true", result);
    }
    
    [Fact]
    public void SafeSerialize_ShouldReturnEmptyObjectForNull()
    {
        // Arrange
        TestObject? obj = null;
        
        // Act
        string result = JsonHelper.SafeSerialize(obj);
        
        // Assert
        Assert.Equal("{}", result);
    }
    
    [Fact]
    public void SafeDeserialize_ShouldDeserializeValidJson()
    {
        // Arrange
        string json = "{\"Name\":\"John\",\"Age\":30,\"IsActive\":true}";
        
        // Act
        var result = JsonHelper.SafeDeserialize<TestObject>(json);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
        Assert.Equal(30, result.Age);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public void SafeDeserialize_ShouldReturnDefaultForInvalidJson()
    {
        // Arrange
        string json = "invalid json";
        
        // Act
        var result = JsonHelper.SafeDeserialize<TestObject>(json);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void SafeDeserialize_ShouldReturnDefaultForNullJson()
    {
        // Arrange
        string? json = null;
        
        // Act
        var result = JsonHelper.SafeDeserialize<TestObject>(json);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void SafeDeserialize_ShouldReturnDefaultForEmptyJson()
    {
        // Arrange
        string json = "";
        
        // Act
        var result = JsonHelper.SafeDeserialize<TestObject>(json);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void PrettyPrint_ShouldFormatValidJson()
    {
        // Arrange
        string json = "{\"Name\":\"John\",\"Age\":30}";
        
        // Act
        string result = JsonHelper.PrettyPrint(json);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("  \"Name\": \"John\"", result);
        Assert.Contains("  \"Age\": 30", result);
        Assert.Contains("\n", result);
    }
    
    [Fact]
    public void PrettyPrint_ShouldReturnOriginalForInvalidJson()
    {
        // Arrange
        string json = "invalid json";
        
        // Act
        string result = JsonHelper.PrettyPrint(json);
        
        // Assert
        Assert.Equal(json, result);
    }
    
    [Fact]
    public void PrettyPrint_ShouldReturnOriginalForNullJson()
    {
        // Arrange
        string? json = null;
        
        // Act
        string result = JsonHelper.PrettyPrint(json);
        
        // Assert
        Assert.Equal(json, result);
    }
    
    [Fact]
    public void PrettyPrint_ShouldHandleComplexObject()
    {
        // Arrange
        var obj = new 
        { 
            User = new TestObject { Name = "John", Age = 30, IsActive = true },
            Items = new[] { "item1", "item2", "item3" },
            Count = 3
        };
        string json = JsonHelper.SafeSerialize(obj);
        
        // Act
        string result = JsonHelper.PrettyPrint(json);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("\n", result);
        Assert.Contains("  \"User\":", result);
        Assert.Contains("  \"Items\":", result);
    }
}

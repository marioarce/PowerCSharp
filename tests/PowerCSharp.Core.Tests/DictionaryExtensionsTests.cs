using PowerCSharp.Core.Collections;
using Xunit;

namespace PowerCSharp.Core.Tests;

public class DictionaryExtensionsTests
{
    [Fact]
    public void AddOrUpdate_WithNewKey_ShouldAddNewEntry()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "existing", 1 }
        };

        // Act
        dictionary.AddOrUpdate("new", 2);

        // Assert
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.ContainsKey("new"));
        Assert.Equal(2, dictionary["new"]);
    }

    [Fact]
    public void AddOrUpdate_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "existing", 1 }
        };

        // Act
        dictionary.AddOrUpdate("existing", 5);

        // Assert
        Assert.Equal(1, dictionary.Count);
        Assert.Equal(5, dictionary["existing"]);
    }

    [Fact]
    public void AddOrUpdate_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        Dictionary<string, int>? nullDictionary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullDictionary!.AddOrUpdate("key", 1));
    }

    [Fact]
    public void AddOrUpdate_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate(null!, 1));
    }

    [Fact]
    public void AddOrUpdate_WithDifferentValueTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var stringDict = new Dictionary<string, string>();
        var objectDict = new Dictionary<string, object>();

        // Act
        stringDict.AddOrUpdate("test", "value");
        objectDict.AddOrUpdate("test", new { Name = "Test" });

        // Assert
        Assert.Equal("value", stringDict["test"]);
        Assert.NotNull(objectDict["test"]);
    }

    [Fact]
    public void TryAdd_WithNewKey_ShouldAddAndReturnTrue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "existing", 1 }
        };

        // Act
        var result = dictionary.TryAdd("new", 2);

        // Assert
        Assert.True(result);
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.ContainsKey("new"));
        Assert.Equal(2, dictionary["new"]);
    }

    [Fact]
    public void TryAdd_WithExistingKey_ShouldNotAddAndReturnFalse()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "existing", 1 }
        };

        // Act
        var result = dictionary.TryAdd("existing", 5);

        // Assert
        Assert.False(result);
        Assert.Equal(1, dictionary.Count);
        Assert.Equal(1, dictionary["existing"]); // Value should remain unchanged
    }

    [Fact]
    public void TryAdd_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        Dictionary<string, int>? nullDictionary = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullDictionary!.TryAdd("key", 1));
    }

    [Fact]
    public void TryAdd_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dictionary.TryAdd(null!, 1));
    }

    [Fact]
    public void TryAdd_WithDifferentValueTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var stringDict = new Dictionary<string, string>();
        var objectDict = new Dictionary<string, object>();

        // Act
        var stringResult = stringDict.TryAdd("test", "value");
        var objectResult = objectDict.TryAdd("test", new { Name = "Test" });

        // Assert
        Assert.True(stringResult);
        Assert.True(objectResult);
        Assert.Equal("value", stringDict["test"]);
        Assert.NotNull(objectDict["test"]);
    }

    [Fact]
    public void AddOrUpdateAndTryAdd_CombinedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act
        var tryAddResult1 = dictionary.TryAdd("key1", 10);
        var tryAddResult2 = dictionary.TryAdd("key2", 20);
        
        dictionary.AddOrUpdate("key1", 15); // Update existing
        dictionary.AddOrUpdate("key3", 30);  // Add new
        
        var tryAddResult3 = dictionary.TryAdd("key1", 99); // Try to add existing
        var tryAddResult4 = dictionary.TryAdd("key4", 40); // Add new

        // Assert
        Assert.True(tryAddResult1);
        Assert.True(tryAddResult2);
        Assert.False(tryAddResult3);
        Assert.True(tryAddResult4);
        
        Assert.Equal(4, dictionary.Count);
        Assert.Equal(15, dictionary["key1"]); // Updated value
        Assert.Equal(20, dictionary["key2"]);
        Assert.Equal(30, dictionary["key3"]);
        Assert.Equal(40, dictionary["key4"]);
    }

    [Fact]
    public void AddOrUpdate_WithComplexKeyTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var dictionary = new Dictionary<Tuple<int, string>, string>();

        var key1 = Tuple.Create(1, "one");
        var key2 = Tuple.Create(2, "two");

        // Act
        dictionary.AddOrUpdate(key1, "value1");
        dictionary.AddOrUpdate(key2, "value2");
        dictionary.AddOrUpdate(key1, "updated_value1");

        // Assert
        Assert.Equal(2, dictionary.Count);
        Assert.Equal("updated_value1", dictionary[key1]);
        Assert.Equal("value2", dictionary[key2]);
    }

    [Fact]
    public void TryAdd_WithComplexKeyTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var dictionary = new Dictionary<Tuple<int, string>, string>();

        var key1 = Tuple.Create(1, "one");
        var key2 = Tuple.Create(2, "two");

        // Act
        var result1 = dictionary.TryAdd(key1, "value1");
        var result2 = dictionary.TryAdd(key2, "value2");
        var result3 = dictionary.TryAdd(key1, "duplicate");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);
        Assert.Equal(2, dictionary.Count);
        Assert.Equal("value1", dictionary[key1]); // Original value preserved
        Assert.Equal("value2", dictionary[key2]);
    }
}

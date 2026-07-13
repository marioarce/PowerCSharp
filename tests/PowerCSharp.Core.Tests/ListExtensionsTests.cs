using PowerCSharp.Core.Collections;
using Xunit;

namespace PowerCSharp.Core.Tests;

public class ListExtensionsTests
{
    #region DistinctBy Tests

    [Fact]
    public void DistinctBy_WithNullList_ShouldReturnNull()
    {
        // Arrange
        List<int>? nullList = null;

        // Act
        var result = nullList.DistinctBy(x => x);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DistinctBy_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<int>();

        // Act
        var result = emptyList.DistinctBy(x => x);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void DistinctBy_WithNoDuplicates_ShouldReturnAllItems()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = list.DistinctBy(x => x);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(list, result);
    }

    [Fact]
    public void DistinctBy_WithDuplicates_ShouldRemoveDuplicates()
    {
        // Arrange
        var list = new List<int> { 1, 2, 2, 3, 4, 4, 5 };

        // Act
        var result = list.DistinctBy(x => x);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(1, result[0]);
        Assert.Equal(2, result[1]);
        Assert.Equal(3, result[2]);
        Assert.Equal(4, result[3]);
        Assert.Equal(5, result[4]);
    }

    [Fact]
    public void DistinctBy_WithComplexType_ShouldDedulicateBySpecifiedField()
    {
        // Arrange
        var list = new List<TestPerson>
        {
            new TestPerson { Id = 1, Name = "John" },
            new TestPerson { Id = 2, Name = "Jane" },
            new TestPerson { Id = 1, Name = "John Duplicate" },
            new TestPerson { Id = 3, Name = "Bob" },
            new TestPerson { Id = 2, Name = "Jane Duplicate" }
        };

        // Act
        var result = list.DistinctBy(x => x.Id);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("John", result[0].Name); // First occurrence kept
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Jane", result[1].Name); // First occurrence kept
        Assert.Equal(3, result[2].Id);
        Assert.Equal("Bob", result[2].Name);
    }

    [Fact]
    public void DistinctBy_WithStringKey_ShouldDedulicateByStringField()
    {
        // Arrange
        var list = new List<TestPerson>
        {
            new TestPerson { Id = 1, Name = "John" },
            new TestPerson { Id = 2, Name = "Jane" },
            new TestPerson { Id = 3, Name = "John" },
            new TestPerson { Id = 4, Name = "Bob" }
        };

        // Act
        var result = list.DistinctBy(x => x.Name);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("John", result[0].Name);
        Assert.Equal("Jane", result[1].Name);
        Assert.Equal("Bob", result[2].Name);
    }

    [Fact]
    public void DistinctBy_WithAllDuplicates_ShouldReturnSingleItem()
    {
        // Arrange
        var list = new List<int> { 5, 5, 5, 5, 5 };

        // Act
        var result = list.DistinctBy(x => x);

        // Assert
        Assert.Single(result);
        Assert.Equal(5, result[0]);
    }

    [Fact]
    public void DistinctBy_ShouldPreserveOrderOfFirstOccurrences()
    {
        // Arrange
        var list = new List<int> { 3, 1, 2, 1, 3, 2 };

        // Act
        var result = list.DistinctBy(x => x);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result[0]); // First occurrence of 3
        Assert.Equal(1, result[1]); // First occurrence of 1
        Assert.Equal(2, result[2]); // First occurrence of 2
    }

    #endregion

    #region DeepClone Tests

    [Fact]
    public void DeepClone_WithNullList_ShouldReturnNull()
    {
        // Arrange
        List<TestCloneable>? nullList = null;

        // Act
        var result = nullList.DeepClone();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeepClone_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<TestCloneable>();

        // Act
        var result = emptyList.DeepClone();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.NotSame(emptyList, result);
    }

    [Fact]
    public void DeepClone_WithCloneableItems_ShouldCreateDeepCopy()
    {
        // Arrange
        var list = new List<TestCloneable>
        {
            new TestCloneable { Value = 1 },
            new TestCloneable { Value = 2 },
            new TestCloneable { Value = 3 }
        };

        // Act
        var result = list.DeepClone();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(list.Count, result.Count);
        Assert.NotSame(list, result);

        for (int i = 0; i < list.Count; i++)
        {
            Assert.NotSame(list[i], result[i]);
            Assert.Equal(list[i].Value, result[i].Value);
        }
    }

    [Fact]
    public void DeepClone_WithNullItems_ShouldHandleNullItems()
    {
        // Arrange
        var list = new List<TestCloneable?>
        {
            new TestCloneable { Value = 1 },
            null,
            new TestCloneable { Value = 3 }
        };

        // Act
        var result = list.DeepClone();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(list.Count, result.Count);
        Assert.NotNull(result[0]);
        Assert.Null(result[1]);
        Assert.NotNull(result[2]);
    }

    [Fact]
    public void DeepClone_ModifyingCloneShouldNotAffectOriginal()
    {
        // Arrange
        var list = new List<TestCloneable>
        {
            new TestCloneable { Value = 10 },
            new TestCloneable { Value = 20 }
        };

        // Act
        var result = list.DeepClone();
        result[0].Value = 999;

        // Assert
        Assert.Equal(10, list[0].Value);
        Assert.Equal(999, result[0].Value);
    }

    #endregion

    #region Test Helper Classes

    private class TestPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestCloneable : ICloneable
    {
        public int Value { get; set; }

        public object Clone()
        {
            return new TestCloneable { Value = this.Value };
        }
    }

    #endregion
}

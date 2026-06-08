using PowerCSharp.Extensions.Collections;
using Xunit;

namespace PowerCSharp.Extensions.Tests;

public class EnumerableExtensionsTests
{
    [Fact]
    public void FirstOrDefaultSafe_ShouldReturnFirstElementForValidList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        
        // Act
        var result = list.FirstOrDefaultSafe();
        
        // Assert
        Assert.Equal(1, result);
    }
    
    [Fact]
    public void FirstOrDefaultSafe_ShouldReturnDefaultForEmptyList()
    {
        // Arrange
        var list = new List<int>();
        
        // Act
        var result = list.FirstOrDefaultSafe();
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public void FirstOrDefaultSafe_ShouldReturnDefaultForNullList()
    {
        // Arrange
        List<int>? list = null;
        
        // Act
        var result = list.FirstOrDefaultSafe();
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public void FirstOrDefaultSafe_ShouldReturnCustomDefaultForEmptyList()
    {
        // Arrange
        var list = new List<int>();
        int defaultValue = -1;
        
        // Act
        var result = list.FirstOrDefaultSafe(defaultValue);
        
        // Assert
        Assert.Equal(defaultValue, result);
    }
    
    [Fact]
    public void FirstOrDefaultSafe_ShouldReturnCustomDefaultForNullList()
    {
        // Arrange
        List<int>? list = null;
        int defaultValue = -1;
        
        // Act
        var result = list.FirstOrDefaultSafe(defaultValue);
        
        // Assert
        Assert.Equal(defaultValue, result);
    }
    
    [Fact]
    public void FirstOrDefaultSafe_ShouldWorkWithStrings()
    {
        // Arrange
        var list = new List<string> { "first", "second", "third" };
        
        // Act
        var result = list.FirstOrDefaultSafe("default");
        
        // Assert
        Assert.Equal("first", result);
    }
    
    [Fact]
    public void FirstOrDefaultSafe_ShouldReturnDefaultForEmptyStringList()
    {
        // Arrange
        var list = new List<string>();
        
        // Act
        var result = list.FirstOrDefaultSafe("default");
        
        // Assert
        Assert.Equal("default", result);
    }
    
    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseForNonEmptyList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        
        // Act
        bool result = list.IsNullOrEmpty();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueForEmptyList()
    {
        // Arrange
        var list = new List<int>();
        
        // Act
        bool result = list.IsNullOrEmpty();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrueForNullList()
    {
        // Arrange
        List<int>? list = null;
        
        // Act
        bool result = list.IsNullOrEmpty();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseForListWithZero()
    {
        // Arrange
        var list = new List<int> { 0 };
        
        // Act
        bool result = list.IsNullOrEmpty();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void IsNullOrEmpty_ShouldReturnFalseForListWithNullItems()
    {
        // Arrange
        var list = new List<string?> { null, null, null };
        
        // Act
        bool result = list.IsNullOrEmpty();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Page_ShouldReturnCorrectPageForValidParameters()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Act
        var result = list.Page(2, 3);
        
        // Assert
        Assert.Equal(new List<int> { 4, 5, 6 }, result);
    }
    
    [Fact]
    public void Page_ShouldReturnEmptyForPageLessThanOne()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        
        // Act
        var result = list.Page(0, 2);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Page_ShouldReturnEmptyForPageSizeLessThanOne()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        
        // Act
        var result = list.Page(1, 0);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Page_ShouldReturnEmptyForNullList()
    {
        // Arrange
        List<int>? list = null;
        
        // Act
        var result = list.Page(1, 2);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Page_ShouldReturnFirstPage()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Act
        var result = list.Page(1, 4);
        
        // Assert
        Assert.Equal(new List<int> { 1, 2, 3, 4 }, result);
    }
    
    [Fact]
    public void Page_ShouldReturnLastPage()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Act
        var result = list.Page(3, 4);
        
        // Assert
        Assert.Equal(new List<int> { 9, 10 }, result);
    }
    
    [Fact]
    public void Page_ShouldReturnEmptyForPageBeyondEnd()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        
        // Act
        var result = list.Page(10, 2);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Page_ShouldWorkWithStrings()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c", "d", "e", "f" };
        
        // Act
        var result = list.Page(2, 2);
        
        // Assert
        Assert.Equal(new List<string> { "c", "d" }, result);
    }
}

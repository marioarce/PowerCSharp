using System;
using PowerCSharp.Core;
using Xunit;

namespace PowerCSharp.Core.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void IsNullOrWhiteSpace_ShouldReturnTrueForNull()
    {
        // Arrange
        string? nullString = null;
        
        // Act
        bool result = nullString.IsNullOrWhiteSpace();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsNullOrWhiteSpace_ShouldReturnTrueForEmptyString()
    {
        // Arrange
        string emptyString = "";
        
        // Act
        bool result = emptyString.IsNullOrWhiteSpace();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsNullOrWhiteSpace_ShouldReturnTrueForWhitespace()
    {
        // Arrange
        string whitespaceString = "   \t\n  ";
        
        // Act
        bool result = whitespaceString.IsNullOrWhiteSpace();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void IsNullOrWhiteSpace_ShouldReturnFalseForValidString()
    {
        // Arrange
        string validString = "Hello World";
        
        // Act
        bool result = validString.IsNullOrWhiteSpace();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void SafeSubstring_ShouldReturnSubstringForValidParameters()
    {
        // Arrange
        string text = "Hello World";
        
        // Act
        string result = text.SafeSubstring(0, 5);
        
        // Assert
        Assert.Equal("Hello", result);
    }
    
    [Fact]
    public void SafeSubstring_ShouldReturnEmptyForInvalidStartIndex()
    {
        // Arrange
        string text = "Hello World";
        
        // Act
        string result = text.SafeSubstring(20, 5);
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void SafeSubstring_ShouldReturnEmptyForInvalidLength()
    {
        // Arrange
        string text = "Hello World";
        
        // Act
        string result = text.SafeSubstring(0, -1);
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void SafeSubstring_ShouldReturnPartialForLengthBeyondBounds()
    {
        // Arrange
        string text = "Hello";
        
        // Act
        string result = text.SafeSubstring(2, 10);
        
        // Assert
        Assert.Equal("llo", result);
    }
    
    [Fact]
    public void SafeSubstring_ShouldReturnEmptyForNullString()
    {
        // Arrange
        string? nullString = null;
        
        // Act
        string result = nullString.SafeSubstring(0, 5);
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void ToTitleCase_ShouldConvertToTitleCase()
    {
        // Arrange
        string text = "hello world from powercsharp";
        
        // Act
        string result = text.ToTitleCase();
        
        // Assert
        Assert.Equal("Hello World From Powercsharp", result);
    }
    
    [Fact]
    public void ToTitleCase_ShouldHandleEmptyString()
    {
        // Arrange
        string emptyString = "";
        
        // Act
        string result = emptyString.ToTitleCase();
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void ToTitleCase_ShouldHandleNullString()
    {
        // Arrange
        string? nullString = null;
        
        // Act
        string result = nullString.ToTitleCase();
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Fact]
    public void ToTitleCase_ShouldHandleSingleWord()
    {
        // Arrange
        string singleWord = "hello";
        
        // Act
        string result = singleWord.ToTitleCase();
        
        // Assert
        Assert.Equal("Hello", result);
    }
    
    [Fact]
    public void ToTitleCase_ShouldHandleMultipleSpaces()
    {
        // Arrange
        string multipleSpaces = "hello   world";
        
        // Act
        string result = multipleSpaces.ToTitleCase();
        
        // Assert
        Assert.Equal("Hello World", result);
    }
}
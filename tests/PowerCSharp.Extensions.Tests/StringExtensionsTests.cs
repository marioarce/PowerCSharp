using PowerCSharp.Extensions.Strings;
using Xunit;

namespace PowerCSharp.Extensions.Tests;

public class StringExtensionsTests
{
    #region Coalesce Tests

    [Fact]
    public void Coalesce_WithNonNullRawValue_ShouldReturnRawValue()
    {
        // Arrange
        string rawValue = "hello";
        string fallback = "fallback";

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Equal(rawValue, result);
    }

    [Fact]
    public void Coalesce_WithEmptyRawValue_ShouldReturnFallback()
    {
        // Arrange
        string rawValue = "";
        string fallback = "fallback";

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Equal(fallback, result);
    }

    [Fact]
    public void Coalesce_WithNullRawValue_ShouldReturnFallback()
    {
        // Arrange
        string? rawValue = null;
        string fallback = "fallback";

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Equal(fallback, result);
    }

    [Fact]
    public void Coalesce_WithWhitespaceRawValue_ShouldReturnRawValue()
    {
        // Arrange
        string rawValue = "   ";
        string fallback = "fallback";

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Equal(rawValue, result);
    }

    [Fact]
    public void Coalesce_WithNullFallback_ShouldReturnFallback()
    {
        // Arrange
        string rawValue = "";
        string? fallback = null;

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Coalesce_WithBothNull_ShouldReturnNull()
    {
        // Arrange
        string? rawValue = null;
        string? fallback = null;

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Coalesce_WithBothEmpty_ShouldReturnFallback()
    {
        // Arrange
        string rawValue = "";
        string fallback = "";

        // Act
        var result = rawValue.Coalesce(fallback);

        // Assert
        Assert.Equal(fallback, result);
    }

    #endregion
}

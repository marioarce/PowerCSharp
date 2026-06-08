using System;
using System.Collections.Generic;
using Xunit;
using PowerCSharp.Compatibility.Extensions;

namespace PowerCSharp.Compatibility.Extensions.Tests;

/// <summary>
/// Unit tests for StringExtensions class in PowerCSharp.Compatibility.Extensions namespace
/// </summary>
public class StringExtensionsTests
{
    [Fact]
    public void AppendQueryParameters_WithValidUrlAndParameters_ShouldAppendCorrectly()
    {
        // Arrange
        string url = "https://example.com";
        var parameters = new Dictionary<string, string>
        {
            { "param1", "value1" },
            { "param2", "value2" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Contains("param1=value1", result);
        Assert.Contains("param2=value2", result);
    }

    [Fact]
    public void AppendQueryParameters_WithExistingQuery_ShouldAppendCorrectly()
    {
        // Arrange
        string url = "https://example.com?existing=value";
        var parameters = new Dictionary<string, string>
        {
            { "param1", "value1" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Contains("existing=value", result);
        Assert.Contains("param1=value1", result);
    }

    [Fact]
    public void AppendQueryParameters_WithEmptyUrl_ShouldReturnEmpty()
    {
        // Arrange
        string url = "";
        var parameters = new Dictionary<string, string>
        {
            { "param1", "value1" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void AppendQueryParameters_WithNullParameters_ShouldReturnOriginalUrl()
    {
        // Arrange
        string url = "https://example.com";
        Dictionary<string, string> parameters = null;

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void AppendQueryParameters_WithEmptyParameters_ShouldReturnOriginalUrl()
    {
        // Arrange
        string url = "https://example.com";
        var parameters = new Dictionary<string, string>();

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void AppendQueryParameters_WithNullUrl_ShouldReturnNull()
    {
        // Arrange
        string url = null;
        var parameters = new Dictionary<string, string>
        {
            { "param1", "value1" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void AppendQueryParameters_WithSpecialCharacters_ShouldEncodeCorrectly()
    {
        // Arrange
        string url = "https://example.com";
        var parameters = new Dictionary<string, string>
        {
            { "search", "hello world" },
            { "filter", "category=test" },
            { "url", "https://test.com" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Contains("search=hello world", result);
        Assert.Contains("filter=category=test", result);
        Assert.Contains("url=https://test.com", result);
    }

    [Fact]
    public void AppendQueryParameters_WithMultipleParameters_ShouldMaintainOrder()
    {
        // Arrange
        string url = "https://example.com";
        var parameters = new Dictionary<string, string>
        {
            { "first", "1" },
            { "second", "2" },
            { "third", "3" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Contains("first=1", result);
        Assert.Contains("second=2", result);
        Assert.Contains("third=3", result);
        Assert.True(result.StartsWith("https://example.com?"));
        Assert.Contains("&", result); // Should have separators between parameters
    }

    [Fact]
    public void AppendQueryParameters_WithComplexUrl_ShouldAppendCorrectly()
    {
        // Arrange
        string url = "https://api.example.com/v1/users?active=true";
        var parameters = new Dictionary<string, string>
        {
            { "page", "1" },
            { "pageSize", "10" }
        };

        // Act
        string result = url.AppendQueryParameters(parameters);

        // Assert
        Assert.Contains("active=true", result);
        Assert.Contains("page=1", result);
        Assert.Contains("pageSize=10", result);
    }
}

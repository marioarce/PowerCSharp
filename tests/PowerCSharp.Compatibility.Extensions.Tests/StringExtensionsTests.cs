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
}

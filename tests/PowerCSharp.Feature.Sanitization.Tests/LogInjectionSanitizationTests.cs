using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>
/// Covers <see cref="SanitizationEngine.SanitizeForLogInjection"/> (CWE-117). Every call passes an
/// explicit <see cref="SanitizationSettings"/> instance rather than relying on the ambient
/// configuration provider, so these tests are independent of any DI/module wiring under test
/// elsewhere in this assembly.
/// </summary>
public class LogInjectionSanitizationTests
{
    [Fact]
    public void Null_Input_Returns_Unchanged_Empty()
    {
        var result = SanitizationEngine.SanitizeForLogInjection(null, new SanitizationSettings());

        Assert.False(result.WasModified);
        Assert.Equal(string.Empty, result.SanitizedValue);
        Assert.Equal(SanitizationType.LogInjection, result.SanitizationType);
    }

    [Fact]
    public void Empty_Input_Returns_Unchanged_Empty()
    {
        var result = SanitizationEngine.SanitizeForLogInjection(string.Empty, new SanitizationSettings());

        Assert.False(result.WasModified);
        Assert.Equal(string.Empty, result.SanitizedValue);
    }

    [Fact]
    public void Input_Without_Control_Characters_Is_Unchanged()
    {
        var result = SanitizationEngine.SanitizeForLogInjection("a perfectly normal log line", new SanitizationSettings());

        Assert.False(result.WasModified);
        Assert.Equal("a perfectly normal log line", result.SanitizedValue);
    }

    [Fact]
    public void Disabled_Setting_Passes_Control_Characters_Through()
    {
        var settings = new SanitizationSettings { EnableLogSanitization = false };

        var result = SanitizationEngine.SanitizeForLogInjection("line1\r\nline2", settings);

        Assert.False(result.WasModified);
        Assert.Equal("line1\r\nline2", result.SanitizedValue);
    }

    [Fact]
    public void Remove_Strategy_Strips_CrLf_And_Html_Encodes_Remainder()
    {
        var settings = new SanitizationSettings { LogSanitizationStrategy = SanitizationStrategy.Remove };

        var result = SanitizationEngine.SanitizeForLogInjection("value\r\nwith<tag>", settings);

        Assert.True(result.WasModified);
        Assert.DoesNotContain('\r', result.SanitizedValue);
        Assert.DoesNotContain('\n', result.SanitizedValue);
        Assert.Equal("valuewith&lt;tag&gt;", result.SanitizedValue);
    }

    [Fact]
    public void ReplaceWithSpace_Strategy_Replaces_Control_Characters_With_Spaces()
    {
        var settings = new SanitizationSettings { LogSanitizationStrategy = SanitizationStrategy.ReplaceWithSpace };

        var result = SanitizationEngine.SanitizeForLogInjection("a\r\nb", settings);

        Assert.True(result.WasModified);
        Assert.Equal("a  b", result.SanitizedValue);
    }

    [Fact]
    public void PreserveTabCharacters_Keeps_Tabs_But_Still_Removes_Other_Control_Characters()
    {
        var settings = new SanitizationSettings
        {
            LogSanitizationStrategy = SanitizationStrategy.Remove,
            PreserveTabCharacters = true
        };

        var result = SanitizationEngine.SanitizeForLogInjection("a\tb\r\nc", settings);

        Assert.Contains('\t', result.SanitizedValue);
        Assert.DoesNotContain('\r', result.SanitizedValue);
        Assert.DoesNotContain('\n', result.SanitizedValue);
        Assert.Equal("a\tbc", result.SanitizedValue);
    }

    [Theory]
    [InlineData(SanitizationStrategy.HtmlEncode)]
    [InlineData(SanitizationStrategy.UrlEncode)]
    [InlineData(SanitizationStrategy.JsonEncode)]
    public void Encoding_Strategies_Remove_Raw_Control_Characters_Without_Throwing(SanitizationStrategy strategy)
    {
        var settings = new SanitizationSettings { LogSanitizationStrategy = strategy };

        var result = SanitizationEngine.SanitizeForLogInjection("inject\r\nme<script>", settings);

        Assert.True(result.WasModified);
        Assert.DoesNotContain('\r', result.SanitizedValue);
        Assert.DoesNotContain('\n', result.SanitizedValue);
    }

    [Fact]
    public void MaxSanitizedStringLength_Truncates_Output()
    {
        var settings = new SanitizationSettings { MaxSanitizedStringLength = 5 };

        var result = SanitizationEngine.SanitizeForLogInjection("abc\r\ndefghijk", settings);

        Assert.True(result.SanitizedValue.Length <= 5);
    }
}

using PowerCSharp.Feature.Sanitization.Abstractions;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>Covers the <see cref="SanitizationExtensions"/> string extension methods.</summary>
public class SanitizationExtensionsTests
{
    [Fact]
    public void SanitizeForLog_Strips_Control_Characters()
    {
        var result = "line1\r\nline2".SanitizeForLog(new SanitizationSettings());

        Assert.DoesNotContain('\r', result);
        Assert.DoesNotContain('\n', result);
    }

    [Fact]
    public void SanitizeForLogWithDetails_Reports_Whether_Modified()
    {
        var result = "clean text".SanitizeForLogWithDetails(new SanitizationSettings());

        Assert.False(result.WasModified);
    }

    [Fact]
    public void SanitizeForFilePath_Returns_Sanitized_Path_When_Valid()
    {
        var result = "folder/file.txt".SanitizeForFilePath(new SanitizationSettings());

        Assert.Equal("folder/file.txt", result);
    }

    [Fact]
    public void SanitizeForFilePath_Throws_When_Rejected_Under_Strict_Validation()
    {
        Assert.Throws<InvalidOperationException>(() => "../../etc/passwd".SanitizeForFilePath(new SanitizationSettings()));
    }

    [Fact]
    public void SanitizeForFilePathWithDetails_Reports_Rejection_Without_Throwing()
    {
        var result = "../../etc/passwd".SanitizeForFilePathWithDetails(new SanitizationSettings());

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void SanitizeForSensitiveData_Masks_Secrets()
    {
        var result = "password=SuperSecret123".SanitizeForSensitiveData(new SanitizationSettings());

        Assert.DoesNotContain("SuperSecret123", result);
    }

    [Fact]
    public void SanitizeForRegexInjection_Returns_Pattern_When_Safe()
    {
        var result = "^[a-z]+$".SanitizeForRegexInjection(new SanitizationSettings());

        Assert.Equal("^[a-z]+$", result);
    }

    [Fact]
    public void SanitizeForRegexInjection_Returns_Empty_When_Rejected()
    {
        var result = "(a+)+$".SanitizeForRegexInjection(new SanitizationSettings());

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Mask_Single_Argument_Overload_Masks_Middle_Portion()
    {
        var result = "abcdefghijklmnop".Mask('*');

        Assert.StartsWith("abcd", result);
        Assert.EndsWith("mnop", result);
        Assert.Contains('*', result);
    }

    [Fact]
    public void Mask_With_Visible_Length_Masks_Everything_When_Value_Too_Short()
    {
        var result = "ab".Mask(4, '*');

        Assert.Equal("**", result);
    }

    [Fact]
    public void Mask_Null_Or_Empty_Returns_Input_Unchanged()
    {
        Assert.Equal(string.Empty, string.Empty.Mask('*'));
    }
}

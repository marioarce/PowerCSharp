using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>
/// Covers <see cref="SanitizationEngine.SanitizeForRegexInjection"/> (CWE-400/CWE-730). Every call
/// passes an explicit <see cref="SanitizationSettings"/> instance rather than relying on the
/// ambient configuration provider.
/// </summary>
public class RegexInjectionSanitizationTests
{
    [Fact]
    public void Null_Pattern_Returns_Unchanged_Empty()
    {
        var result = SanitizationEngine.SanitizeForRegexInjection(null, new SanitizationSettings());

        Assert.False(result.IsRejected);
        Assert.Equal(string.Empty, result.SanitizedValue);
        Assert.Equal(SanitizationType.RegexInjection, result.SanitizationType);
    }

    [Fact]
    public void Simple_Safe_Pattern_Is_Accepted_Unchanged()
    {
        const string pattern = @"^[A-Za-z0-9_-]{1,32}$";

        var result = SanitizationEngine.SanitizeForRegexInjection(pattern, new SanitizationSettings());

        Assert.False(result.IsRejected);
        Assert.Equal(pattern, result.SanitizedValue);
    }

    [Fact]
    public void Invalid_Regex_Syntax_Is_Rejected()
    {
        var result = SanitizationEngine.SanitizeForRegexInjection("(unclosed[group", new SanitizationSettings());

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Nested_Quantifiers_Are_Rejected_By_Default()
    {
        // Default settings disallow nested quantifiers: the classic catastrophic-backtracking shape.
        var result = SanitizationEngine.SanitizeForRegexInjection("(a+)+$", new SanitizationSettings());

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Nested_Quantifiers_Are_Accepted_When_Explicitly_Allowed()
    {
        var settings = new SanitizationSettings { AllowNestedQuantifiers = true };

        var result = SanitizationEngine.SanitizeForRegexInjection("(a+)+$", settings);

        Assert.False(result.IsRejected);
    }

    [Fact]
    public void Pattern_Exceeding_Max_Length_Is_Rejected()
    {
        var settings = new SanitizationSettings { MaxRegexPatternLength = 10 };

        var result = SanitizationEngine.SanitizeForRegexInjection("abcdefghijklmnopqrstuvwxyz", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Quantifiers_Rejected_When_Disallowed()
    {
        var settings = new SanitizationSettings { AllowQuantifiers = false };

        var result = SanitizationEngine.SanitizeForRegexInjection("a+b", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Backreferences_Rejected_When_Disallowed()
    {
        var settings = new SanitizationSettings { AllowBackreferences = false };

        var result = SanitizationEngine.SanitizeForRegexInjection(@"(a)\1", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Lookarounds_Rejected_When_Disallowed()
    {
        var settings = new SanitizationSettings { AllowLookarounds = false };

        var result = SanitizationEngine.SanitizeForRegexInjection("a(?=b)", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Unicode_Categories_Rejected_When_Disallowed()
    {
        var settings = new SanitizationSettings { AllowUnicodeCategories = false };

        var result = SanitizationEngine.SanitizeForRegexInjection(@"\p{L}+", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Disabled_Setting_Accepts_Any_Pattern_Unvalidated()
    {
        var settings = new SanitizationSettings { EnableRegexSanitization = false };

        var result = SanitizationEngine.SanitizeForRegexInjection("(a+)+$", settings);

        Assert.False(result.IsRejected);
        Assert.Equal("(a+)+$", result.SanitizedValue);
    }
}

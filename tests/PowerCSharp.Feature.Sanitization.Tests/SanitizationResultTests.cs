using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>Covers the factory methods and value-equality contracts of <see cref="SanitizationResult"/> and <see cref="SensitiveDataResult"/>.</summary>
public class SanitizationResultTests
{
    [Fact]
    public void Unchanged_Sets_WasModified_False_And_IsRejected_False()
    {
        var result = SanitizationResult.Unchanged("value", SanitizationType.LogInjection);

        Assert.False(result.WasModified);
        Assert.False(result.IsRejected);
        Assert.Equal("value", result.SanitizedValue);
        Assert.Equal(TimeSpan.Zero, result.ProcessingTime);
    }

    [Fact]
    public void Modified_Sets_WasModified_True()
    {
        var result = SanitizationResult.Modified("clean", SanitizationType.LogInjection, TimeSpan.FromMilliseconds(2));

        Assert.True(result.WasModified);
        Assert.False(result.IsRejected);
        Assert.Equal("clean", result.SanitizedValue);
    }

    [Fact]
    public void Rejected_Sets_IsRejected_True_And_Empty_Value()
    {
        var result = SanitizationResult.Rejected(SanitizationType.FilePath, TimeSpan.FromMilliseconds(1));

        Assert.True(result.IsRejected);
        Assert.Equal(string.Empty, result.SanitizedValue);
    }

    [Fact]
    public void Two_Results_With_Same_Values_Are_Equal()
    {
        var left = SanitizationResult.Modified("value", SanitizationType.LogInjection, TimeSpan.FromMilliseconds(5));
        var right = SanitizationResult.Modified("value", SanitizationType.LogInjection, TimeSpan.FromMilliseconds(5));

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Results_With_Different_Values_Are_Not_Equal()
    {
        var left = SanitizationResult.Unchanged("a", SanitizationType.LogInjection);
        var right = SanitizationResult.Unchanged("b", SanitizationType.LogInjection);

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }

    [Fact]
    public void SensitiveDataResult_Unchanged_Has_Correct_Defaults()
    {
        var result = SensitiveDataResult.Unchanged("value");

        Assert.False(result.WasModified);
        Assert.False(result.IsRejected);
        Assert.Equal(SanitizationType.SensitiveData, result.SanitizationType);
    }

    [Fact]
    public void SensitiveDataResult_Modified_Has_Correct_Values()
    {
        var result = SensitiveDataResult.Modified("masked", TimeSpan.FromMilliseconds(3));

        Assert.True(result.WasModified);
        Assert.Equal("masked", result.SanitizedValue);
    }

    [Fact]
    public void SensitiveDataResults_With_Same_Values_Are_Equal()
    {
        var left = SensitiveDataResult.Unchanged("value");
        var right = SensitiveDataResult.Unchanged("value");

        Assert.Equal(left, right);
        Assert.True(left == right);
    }
}

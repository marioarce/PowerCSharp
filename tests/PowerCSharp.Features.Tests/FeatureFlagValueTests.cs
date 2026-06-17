using PowerCSharp.Features.Abstractions;
using Xunit;

namespace PowerCSharp.Features.Tests;

public class FeatureFlagValueTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("not-a-bool", false)]
    public void AsBoolean_ParsesOrFallsBack(string raw, bool expected)
    {
        var value = new FeatureFlagValue(raw, FeatureFlagSource.Configuration);
        Assert.Equal(expected, value.AsBoolean());
    }

    [Fact]
    public void AsInt32_ParsesInvariant()
    {
        var value = new FeatureFlagValue("42", FeatureFlagSource.Configuration);
        Assert.Equal(42, value.AsInt32());
    }

    [Fact]
    public void AsEnum_IsCaseInsensitive()
    {
        var value = new FeatureFlagValue("configuration", FeatureFlagSource.Override);
        Assert.Equal(FeatureFlagSource.Configuration, value.AsEnum<FeatureFlagSource>());
    }

    [Fact]
    public void Missing_HasNoValue()
    {
        Assert.False(FeatureFlagValue.Missing.HasValue);
        Assert.Equal(FeatureFlagSource.Default, FeatureFlagValue.Missing.Source);
    }
}

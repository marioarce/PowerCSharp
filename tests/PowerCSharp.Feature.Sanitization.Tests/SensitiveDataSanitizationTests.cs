using PowerCSharp.Feature.Sanitization.Abstractions;
using PowerCSharp.Feature.Sanitization.Abstractions.Enums;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>
/// Covers <see cref="SanitizationEngine.SanitizeForSensitiveData"/> (CWE-200). Every call passes an
/// explicit <see cref="SanitizationSettings"/> instance rather than relying on the ambient
/// configuration provider. Expected masked output was derived by tracing the exact regex/masking
/// pipeline order in <c>SanitizationEngine.SensitiveData.cs</c> (verified against a reference
/// implementation), not guessed.
/// </summary>
public class SensitiveDataSanitizationTests
{
    [Fact]
    public void Null_Input_Returns_Unchanged_Empty()
    {
        var result = SanitizationEngine.SanitizeForSensitiveData(null, new SanitizationSettings());

        Assert.False(result.WasModified);
        Assert.Equal(string.Empty, result.SanitizedValue);
        Assert.Equal(SanitizationType.SensitiveData, result.SanitizationType);
    }

    [Fact]
    public void Innocuous_Text_Is_Left_Unchanged()
    {
        const string text = "Hello world, this is a normal log message.";

        var result = SanitizationEngine.SanitizeForSensitiveData(text, new SanitizationSettings());

        Assert.False(result.WasModified);
        Assert.Equal(text, result.SanitizedValue);
    }

    [Fact]
    public void Disabled_Setting_Passes_Sensitive_Text_Through()
    {
        var settings = new SanitizationSettings { EnableSensitiveDataDetection = false };

        var result = SanitizationEngine.SanitizeForSensitiveData("password=SuperSecret123", settings);

        Assert.False(result.WasModified);
        Assert.Equal("password=SuperSecret123", result.SanitizedValue);
    }

    [Fact]
    public void Password_Key_Value_Is_Masked_But_Key_Name_Is_Preserved()
    {
        var result = SanitizationEngine.SanitizeForSensitiveData("password=SuperSecret123", new SanitizationSettings());

        Assert.True(result.WasModified);
        Assert.StartsWith("password=", result.SanitizedValue);
        Assert.DoesNotContain("SuperSecret123", result.SanitizedValue);
        Assert.Equal("password=Sup********123", result.SanitizedValue);
    }

    [Fact]
    public void Bearer_Token_Is_Masked_But_Prefix_Is_Preserved()
    {
        var result = SanitizationEngine.SanitizeForSensitiveData("Bearer abcdefghijklmnopqrstuvwxyz123456", new SanitizationSettings());

        Assert.True(result.WasModified);
        Assert.StartsWith("Bearer ", result.SanitizedValue);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz123456", result.SanitizedValue);
    }

    [Fact]
    public void Api_Key_Is_Masked_But_Key_Name_Is_Preserved()
    {
        var result = SanitizationEngine.SanitizeForSensitiveData("api_key=abcdefgh12345678", new SanitizationSettings());

        Assert.True(result.WasModified);
        Assert.StartsWith("api_key=", result.SanitizedValue);
        Assert.DoesNotContain("abcdefgh12345678", result.SanitizedValue);
    }

    [Fact]
    public void Windows_Path_Is_Masked()
    {
        var result = SanitizationEngine.SanitizeForSensitiveData(
            @"Connecting to C:\Users\mario\secrets.txt now", new SanitizationSettings());

        Assert.True(result.WasModified);
        Assert.DoesNotContain(@"C:\Users\mario\secrets.txt", result.SanitizedValue);
    }

    [Fact]
    public void Unix_Home_Path_Is_Masked()
    {
        var result = SanitizationEngine.SanitizeForSensitiveData(
            "Path is /home/mario/.ssh/id_rsa for reference", new SanitizationSettings());

        Assert.True(result.WasModified);
        Assert.DoesNotContain("/home/mario/.ssh/id_rsa", result.SanitizedValue);
    }

    [Fact]
    public void Low_Strictness_Does_Not_Flag_Long_Alphanumeric_Strings_Alone()
    {
        var settings = new SanitizationSettings { SensitiveDataDetectionStrictness = SensitiveDataDetectionStrictness.Low };

        var result = SanitizationEngine.SanitizeForSensitiveData(
            "reference id ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", settings);

        Assert.False(result.WasModified);
    }

    [Fact]
    public void Medium_Strictness_Flags_Long_Alphanumeric_Strings()
    {
        var settings = new SanitizationSettings { SensitiveDataDetectionStrictness = SensitiveDataDetectionStrictness.Medium };

        var result = SanitizationEngine.SanitizeForSensitiveData(
            "reference id ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", settings);

        Assert.True(result.WasModified);
    }

    [Fact]
    public void Custom_Mask_Character_Is_Used()
    {
        var settings = new SanitizationSettings { SensitiveDataMaskCharacter = '#' };

        var result = SanitizationEngine.SanitizeForSensitiveData("password=SuperSecret123", settings);

        Assert.Contains('#', result.SanitizedValue);
        Assert.DoesNotContain('*', result.SanitizedValue);
    }
}

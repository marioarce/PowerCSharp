using PowerCSharp.Feature.Sanitization.Abstractions;

namespace PowerCSharp.Feature.Sanitization.Tests;

/// <summary>
/// Covers <see cref="SanitizationEngine.SanitizeForFilePath"/> and
/// <see cref="SanitizationEngine.SanitizeCorrelationIdForPath"/> (CWE-22). Every call passes an
/// explicit <see cref="SanitizationSettings"/> instance rather than relying on the ambient
/// configuration provider.
/// </summary>
public class FilePathSanitizationTests
{
    [Fact]
    public void Null_Input_Returns_Unchanged_Empty()
    {
        var result = SanitizationEngine.SanitizeForFilePath(null, new SanitizationSettings());

        Assert.False(result.WasModified);
        Assert.False(result.IsRejected);
        Assert.Equal(string.Empty, result.SanitizedValue);
    }

    [Fact]
    public void Disabled_Setting_Passes_Path_Through_Unvalidated()
    {
        var settings = new SanitizationSettings { EnableFilePathSanitization = false };

        var result = SanitizationEngine.SanitizeForFilePath("../../etc/passwd", settings);

        Assert.False(result.IsRejected);
        Assert.Equal("../../etc/passwd", result.SanitizedValue);
    }

    [Fact]
    public void Strict_Mode_Accepts_A_Plain_Relative_Path()
    {
        var result = SanitizationEngine.SanitizeForFilePath("folder/file.txt", new SanitizationSettings());

        Assert.False(result.IsRejected);
        Assert.Equal("folder/file.txt", result.SanitizedValue);
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("/etc/passwd")]
    [InlineData(@"C:\Windows\System32\cmd.exe")]
    [InlineData("..%2f..%2fsecret")]
    public void Strict_Mode_Rejects_Traversal_And_Absolute_Paths(string path)
    {
        var result = SanitizationEngine.SanitizeForFilePath(path, new SanitizationSettings());

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Strict_Mode_Rejects_Windows_Reserved_Device_Names()
    {
        var result = SanitizationEngine.SanitizeForFilePath("CON.txt", new SanitizationSettings());

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Strict_Mode_With_Reserved_Name_Validation_Disabled_Accepts_Reserved_Name()
    {
        var settings = new SanitizationSettings { ValidateWindowsReservedNames = false };

        var result = SanitizationEngine.SanitizeForFilePath("CON.txt", settings);

        Assert.False(result.IsRejected);
    }

    [Fact]
    public void Strict_Mode_Extension_Allowlist_Accepts_Matching_Extension()
    {
        var settings = new SanitizationSettings { AllowedFileExtensions = new[] { "txt" } };

        var result = SanitizationEngine.SanitizeForFilePath("notes.txt", settings);

        Assert.False(result.IsRejected);
    }

    [Fact]
    public void Strict_Mode_Extension_Allowlist_Rejects_Non_Matching_Extension()
    {
        var settings = new SanitizationSettings { AllowedFileExtensions = new[] { "txt" } };

        var result = SanitizationEngine.SanitizeForFilePath("notes.exe", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Strict_Mode_Base_Directory_Allowlist_Accepts_Path_Within_Directory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"sanitization-test-{Guid.NewGuid()}");
        var settings = new SanitizationSettings { AllowedBaseDirectories = new[] { baseDir } };

        var result = SanitizationEngine.SanitizeForFilePath("subfolder/file.txt", settings);

        Assert.False(result.IsRejected);
    }

    [Fact]
    public void Strict_Mode_Base_Directory_Allowlist_Rejects_Escape_Attempt()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"sanitization-test-{Guid.NewGuid()}");
        var settings = new SanitizationSettings { AllowedBaseDirectories = new[] { baseDir } };

        var result = SanitizationEngine.SanitizeForFilePath("../escape.txt", settings);

        Assert.True(result.IsRejected);
    }

    [Fact]
    public void Legacy_Mode_Strips_Parent_Directory_References()
    {
        var settings = new SanitizationSettings { UseStrictValidation = false };

        var result = SanitizationEngine.SanitizeForFilePath("a/../b", settings);

        Assert.True(result.WasModified);
        Assert.Equal("a/b", result.SanitizedValue);
    }

    [Fact]
    public void Legacy_Mode_Leaves_Already_Clean_Path_Unchanged()
    {
        var settings = new SanitizationSettings { UseStrictValidation = false };

        var result = SanitizationEngine.SanitizeForFilePath("folder/file.txt", settings);

        Assert.False(result.WasModified);
        Assert.Equal("folder/file.txt", result.SanitizedValue);
    }

    [Fact]
    public void SanitizeCorrelationIdForPath_Replaces_Invalid_Characters()
    {
        // SanitizeCorrelationIdForPath consults Path.GetInvalidFileNameChars(), which is
        // platform-dependent: on Windows it includes ':' and '*', but those are legal in Unix file
        // names, so this assertion sticks to the small set (NUL and '/') that is invalid on every
        // platform .NET targets, to keep the test portable.
        var result = SanitizationEngine.SanitizeCorrelationIdForPath("abc\0def/ghi");

        Assert.DoesNotContain('\0', result);
        Assert.DoesNotContain('/', result);
    }

    [Fact]
    public void SanitizeCorrelationIdForPath_Truncates_To_Fifty_Characters()
    {
        var longId = new string('x', 100);

        var result = SanitizationEngine.SanitizeCorrelationIdForPath(longId);

        Assert.Equal(50, result.Length);
    }

    [Fact]
    public void SanitizeCorrelationIdForPath_Null_Or_Empty_Returns_Empty()
    {
        Assert.Equal(string.Empty, SanitizationEngine.SanitizeCorrelationIdForPath(null!));
        Assert.Equal(string.Empty, SanitizationEngine.SanitizeCorrelationIdForPath(string.Empty));
    }
}

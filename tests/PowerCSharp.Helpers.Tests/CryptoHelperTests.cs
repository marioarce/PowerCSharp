using PowerCSharp.Helpers;
using Xunit;

namespace PowerCSharp.Helpers.Tests;

public class CryptoHelperTests
{
    [Fact]
    public void ComputeSHA256_WithValidInput_ShouldReturnHash()
    {
        // Arrange
        string input = "PowerCSharp";

        // Act
        string result = CryptoHelper.ComputeSHA256(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void ComputeSHA256_WithSameInput_ShouldReturnSameHash()
    {
        // Arrange
        string input = "PowerCSharp";

        // Act
        string result1 = CryptoHelper.ComputeSHA256(input);
        string result2 = CryptoHelper.ComputeSHA256(input);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ComputeSHA256_WithDifferentInput_ShouldReturnDifferentHash()
    {
        // Arrange
        string input1 = "PowerCSharp";
        string input2 = "DifferentInput";

        // Act
        string result1 = CryptoHelper.ComputeSHA256(input1);
        string result2 = CryptoHelper.ComputeSHA256(input2);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void ComputeSHA256_WithEmptyString_ShouldReturnHash()
    {
        // Arrange
        string input = "";

        // Act
        string result = CryptoHelper.ComputeSHA256(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ComputeSHA256_WithLongString_ShouldReturnHash()
    {
        // Arrange
        string input = new string('A', 10000); // Long string

        // Act
        string result = CryptoHelper.ComputeSHA256(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ComputeMD5_WithValidInput_ShouldReturnHash()
    {
        // Arrange
        string input = "PowerCSharp";

        // Act
        string result = CryptoHelper.ComputeMD5(input);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(32, result.Length); // MD5 hash is always 32 characters
        Assert.Matches(@"^[a-f0-9]{32}$", result); // Should be lowercase hex
    }

    [Fact]
    public void ComputeMD5_WithSameInput_ShouldReturnSameHash()
    {
        // Arrange
        string input = "PowerCSharp";

        // Act
        string result1 = CryptoHelper.ComputeMD5(input);
        string result2 = CryptoHelper.ComputeMD5(input);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ComputeMD5_WithDifferentInput_ShouldReturnDifferentHash()
    {
        // Arrange
        string input1 = "PowerCSharp";
        string input2 = "DifferentInput";

        // Act
        string result1 = CryptoHelper.ComputeMD5(input1);
        string result2 = CryptoHelper.ComputeMD5(input2);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void ComputeMD5_WithEmptyString_ShouldReturnKnownHash()
    {
        // Arrange
        string input = "";
        string expectedHash = "d41d8cd98f00b204e9800998ecf8427e"; // Known MD5 hash of empty string

        // Act
        string result = CryptoHelper.ComputeMD5(input);

        // Assert
        Assert.Equal(expectedHash, result);
    }

    [Fact]
    public void ComputeMD5_WithLongString_ShouldReturnHash()
    {
        // Arrange
        string input = new string('B', 10000); // Long string

        // Act
        string result = CryptoHelper.ComputeMD5(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
        Assert.Matches(@"^[a-f0-9]{32}$", result);
    }

    [Fact]
    public void GenerateRandomString_WithValidLength_ShouldReturnCorrectLength()
    {
        // Arrange
        int length = 10;

        // Act
        string result = CryptoHelper.GenerateRandomString(length);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(length, result.Length);
    }

    [Fact]
    public void GenerateRandomString_WithZeroLength_ShouldReturnEmptyString()
    {
        // Arrange
        int length = 0;

        // Act
        string result = CryptoHelper.GenerateRandomString(length);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GenerateRandomString_WithDifferentLengths_ShouldReturnCorrectLengths()
    {
        // Arrange
        int[] lengths = { 1, 5, 10, 20, 50, 100 };

        // Act & Assert
        foreach (int length in lengths)
        {
            string result = CryptoHelper.GenerateRandomString(length);
            Assert.Equal(length, result.Length);
        }
    }

    [Fact]
    public void GenerateRandomString_ShouldContainOnlyValidCharacters()
    {
        // Arrange
        int length = 100;
        string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // Act
        string result = CryptoHelper.GenerateRandomString(length);

        // Assert
        Assert.All(result, c => Assert.Contains(c, validChars));
    }

    [Fact]
    public void GenerateRandomString_WithMultipleCalls_ShouldReturnDifferentStrings()
    {
        // Arrange
        int length = 20;

        // Act
        string result1 = CryptoHelper.GenerateRandomString(length);
        string result2 = CryptoHelper.GenerateRandomString(length);
        string result3 = CryptoHelper.GenerateRandomString(length);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.NotEqual(result2, result3);
        Assert.NotEqual(result1, result3);
    }

    [Fact]
    public void GenerateRandomString_ShouldBeReasonablyRandom()
    {
        // Arrange
        int length = 1000;
        int iterations = 100;
        var uniqueResults = new System.Collections.Generic.HashSet<string>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            string result = CryptoHelper.GenerateRandomString(length);
            uniqueResults.Add(result);
        }

        // Assert
        Assert.Equal(iterations, uniqueResults.Count); // All results should be unique
    }

    [Fact]
    public void CryptoHelper_HashFunctions_ShouldBeCaseSensitive()
    {
        // Arrange
        string input1 = "PowerCSharp";
        string input2 = "powercsharp";

        // Act
        string sha256Result1 = CryptoHelper.ComputeSHA256(input1);
        string sha256Result2 = CryptoHelper.ComputeSHA256(input2);
        string md5Result1 = CryptoHelper.ComputeMD5(input1);
        string md5Result2 = CryptoHelper.ComputeMD5(input2);

        // Assert
        Assert.NotEqual(sha256Result1, sha256Result2);
        Assert.NotEqual(md5Result1, md5Result2);
    }

    [Fact]
    public void CryptoHelper_KnownHashes_ShouldMatchExpectedValues()
    {
        // Arrange
        string input = "hello";
        string expectedMD5 = "5d41402abc4b2a76b9719d911017c592"; // Known MD5 of "hello"

        // Act
        string md5Result = CryptoHelper.ComputeMD5(input);
        string sha256Result = CryptoHelper.ComputeSHA256(input);

        // Assert
        Assert.Equal(expectedMD5, md5Result);
        Assert.NotNull(sha256Result);
        Assert.NotEmpty(sha256Result);
    }
}

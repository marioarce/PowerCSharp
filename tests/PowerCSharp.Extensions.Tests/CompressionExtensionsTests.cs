using System.Text;
using PowerCSharp.Extensions.Compression;
using Xunit;

namespace PowerCSharp.Extensions.Tests;

public class CompressionExtensionsTests
{
    [Fact]
    public void Deflate_ShouldCompressAndDecompressCorrectly()
    {
        // Arrange
        string originalText = "PowerCSharp is a comprehensive library of extensions and utilities for .NET applications. This is a longer text to ensure compression actually reduces the size. " +
                           "We need enough data to make the compression meaningful and test the round-trip functionality properly. " +
                           "This should be sufficient to demonstrate that the compression and decompression work correctly.";
        byte[] originalBytes = Encoding.UTF8.GetBytes(originalText);

        // Act
        byte[]? compressed = originalBytes.Deflate();
        byte[]? decompressed = compressed?.DeflateDecompress();
        string decompressedText = decompressed != null ? Encoding.UTF8.GetString(decompressed) : string.Empty;

        // Assert
        Assert.NotNull(compressed);
        Assert.True(compressed.Length > 0, "Compressed data should not be empty");
        Assert.NotNull(decompressed);
        Assert.Equal(originalText, decompressedText);
        Assert.Equal(originalBytes.Length, decompressed!.Length);
    }

    [Fact]
    public void Deflate_WithNullData_ShouldReturnNull()
    {
        // Arrange
        byte[]? nullData = null;

        // Act
        byte[]? result = nullData.Deflate();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeflateDecompress_WithNullData_ShouldReturnNull()
    {
        // Arrange
        byte[]? nullData = null;

        // Act
        byte[]? result = nullData.DeflateDecompress();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GzipCompress_ShouldCompressAndDecompressCorrectly()
    {
        // Arrange
        string originalText = "PowerCSharp is a comprehensive library of extensions and utilities for .NET applications. This is a longer text to ensure compression actually reduces the size. " +
                           "We need enough data to make the compression meaningful and test the round-trip functionality properly. " +
                           "This should be sufficient to demonstrate that the compression and decompression work correctly.";
        byte[] originalBytes = Encoding.UTF8.GetBytes(originalText);

        // Act
        byte[]? compressed = originalBytes.GzipCompress();
        byte[]? decompressed = compressed?.GzipDecompress();
        string decompressedText = decompressed != null ? Encoding.UTF8.GetString(decompressed) : string.Empty;

        // Assert
        Assert.NotNull(compressed);
        Assert.True(compressed.Length > 0, "Compressed data should not be empty");
        Assert.NotNull(decompressed);
        Assert.Equal(originalText, decompressedText);
        Assert.Equal(originalBytes.Length, decompressed!.Length);
    }

    [Fact]
    public void GzipCompress_WithNullData_ShouldReturnNull()
    {
        // Arrange
        byte[]? nullData = null;

        // Act
        byte[]? result = nullData.GzipCompress();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GzipDecompress_WithNullData_ShouldReturnNull()
    {
        // Arrange
        byte[]? nullData = null;

        // Act
        byte[]? result = nullData.GzipDecompress();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Deflate_ShouldProduceConsistentResults()
    {
        // Arrange
        byte[] originalBytes = Encoding.UTF8.GetBytes("Test data for compression");

        // Act
        byte[]? compressed1 = originalBytes.Deflate();
        byte[]? compressed2 = originalBytes.Deflate();

        // Assert
        Assert.NotNull(compressed1);
        Assert.NotNull(compressed2);
        Assert.Equal(compressed1, compressed2);
    }

    [Fact]
    public void GzipCompress_ShouldProduceConsistentResults()
    {
        // Arrange
        byte[] originalBytes = Encoding.UTF8.GetBytes("Test data for compression");

        // Act
        byte[]? compressed1 = originalBytes.GzipCompress();
        byte[]? compressed2 = originalBytes.GzipCompress();

        // Assert
        Assert.NotNull(compressed1);
        Assert.NotNull(compressed2);
        Assert.Equal(compressed1, compressed2);
    }

    [Fact]
    public void Compression_ShouldHandleEmptyData()
    {
        // Arrange
        byte[] emptyBytes = Array.Empty<byte>();

        // Act & Assert
        byte[]? deflateCompressed = emptyBytes.Deflate();
        byte[]? deflateDecompressed = deflateCompressed?.DeflateDecompress();
        
        byte[]? gzipCompressed = emptyBytes.GzipCompress();
        byte[]? gzipDecompressed = gzipCompressed?.GzipDecompress();

        Assert.NotNull(deflateCompressed);
        Assert.NotNull(deflateDecompressed);
        Assert.Empty(deflateDecompressed);

        Assert.NotNull(gzipCompressed);
        Assert.NotNull(gzipDecompressed);
        Assert.Empty(gzipDecompressed);
    }
}

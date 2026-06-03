using PowerCSharp.Extensions.Compression;

namespace WebSample.Samples.Extensions;

/// <summary>
/// Sample endpoints demonstrating compression utilities
/// </summary>
public static class CompressionSampleEndpoints
{
    /// <summary>
    /// Gets compression demo data
    /// </summary>
    /// <returns>Demo results showing compression and decompression operations</returns>
    public static object GetDemoData()
    {
        string originalText = "PowerCSharp is a comprehensive library of extensions and utilities for .NET applications. It provides helpers for common operations, validation, cryptography, and more.";
        byte[] originalBytes = System.Text.Encoding.UTF8.GetBytes(originalText);
        
        return new
        {
            original = new { text = originalText, size = originalBytes.Length },
            deflate = new { 
                compressed = originalBytes.Deflate(), 
                decompressed = originalBytes.Deflate()?.DeflateDecompress(),
                compressionRatio = originalBytes.Deflate()?.Length != null ? (double)originalBytes.Deflate()!.Length / originalBytes.Length * 100 : 0
            },
            gzip = new { 
                compressed = originalBytes.GzipCompress(), 
                decompressed = originalBytes.GzipCompress()?.GzipDecompress(),
                compressionRatio = originalBytes.GzipCompress()?.Length != null ? (double)originalBytes.GzipCompress()!.Length / originalBytes.Length * 100 : 0
            }
        };
    }
}

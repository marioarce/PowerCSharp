using System.IO;
using System.IO.Compression;

namespace PowerCSharp.Extensions.Compression;

/// <summary>
/// Extension methods for compression operations on byte arrays.
/// </summary>
public static class CompressionExtensions
{
    /// <summary>
    /// Compresses the byte array using Deflate compression algorithm.
    /// </summary>
    /// <param name="data">The byte array to compress.</param>
    /// <returns>The compressed byte array, or null if the input is null.</returns>
    /// <remarks>
    /// This method uses the Deflate algorithm which is a lossless data compression algorithm
    /// that uses a combination of LZ77 and Huffman coding.
    /// </remarks>
    public static byte[]? Deflate(this byte[]? data)
    {
        if (data == null)
        {
            return null;
        }

        using var output = new MemoryStream();
        using (var compressor = new DeflateStream(output, CompressionMode.Compress))
        {
            compressor.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Compresses the byte array using Gzip compression algorithm.
    /// </summary>
    /// <param name="data">The byte array to compress.</param>
    /// <returns>The compressed byte array, or null if the input is null.</returns>
    /// <remarks>
    /// This method uses the Gzip algorithm which is based on the Deflate algorithm
    /// but includes a CRC-32 checksum and additional header information.
    /// Gzip is more widely supported in web browsers and HTTP servers.
    /// </remarks>
    public static byte[]? GzipCompress(this byte[]? data)
    {
        if (data == null)
        {
            return null;
        }

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            gzip.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Decompresses a byte array that was compressed using Deflate compression.
    /// </summary>
    /// <param name="compressedData">The compressed byte array to decompress.</param>
    /// <returns>The decompressed byte array, or null if the input is null.</returns>
    /// <exception cref="InvalidDataException">Thrown when the compressed data is corrupted or invalid.</exception>
    public static byte[]? DeflateDecompress(this byte[]? compressedData)
    {
        if (compressedData == null)
        {
            return null;
        }

        using var input = new MemoryStream(compressedData);
        using var decompressor = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        decompressor.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    /// Decompresses a byte array that was compressed using Gzip compression.
    /// </summary>
    /// <param name="compressedData">The compressed byte array to decompress.</param>
    /// <returns>The decompressed byte array, or null if the input is null.</returns>
    /// <exception cref="InvalidDataException">Thrown when the compressed data is corrupted or invalid.</exception>
    public static byte[]? GzipDecompress(this byte[]? compressedData)
    {
        if (compressedData == null)
        {
            return null;
        }

        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        gzip.CopyTo(output);
        return output.ToArray();
    }
}

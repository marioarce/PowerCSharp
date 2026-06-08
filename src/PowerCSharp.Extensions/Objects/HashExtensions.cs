using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerCSharp.Extensions.Objects;

/// <summary>
/// Provides extension methods for computing hash values from objects.
/// Uses JSON serialization and SHA256 hashing to generate consistent, short hash strings for caching and identification purposes.
/// </summary>
public static class HashExtensions
{
    /// <summary>
    /// JSON serialization options configured for stable, deterministic output.
    /// Includes all fields and properties, handles circular references, and uses camelCase naming for consistency.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,

        // Include all fields + properties
        IncludeFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,

#if NET6_0_OR_GREATER
        // Handle circular references (object graphs with cycles)
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
#endif

        // Prevent runaway recursion (very large nested objects)
        MaxDepth = 64
    };

    /// <summary>
    /// Computes a short hash string from any object by serializing it to JSON and applying SHA256 hashing.
    /// Handles serialization failures gracefully by generating a fallback hash based on the exception and type name.
    /// </summary>
    /// <param name="obj">The object to hash. Can be any serializable type or null.</param>
    /// <returns>
    /// A 16-character hexadecimal hash string representing the object's content.
    /// Returns "null" if the input object is null.
    /// Returns a fallback hash if serialization fails, incorporating the type name and error information.
    /// </returns>
    public static string ComputeHash(this object obj)
    {
        if (obj == null)
        {
            return "null";
        }

        string input;

        try
        {
            input = JsonSerializer.Serialize(obj, _jsonOptions);
        }
        catch (Exception ex)
        {
            // Stable short hash from exception message
            var exMsg = ex.Message ?? string.Empty;
            var exBytes = Encoding.UTF8.GetBytes(exMsg);
            var exHash = ComputeSha256Hash(exBytes);

            // Take first 4 bytes = 8 hex chars
            var shortExHash = ToHexString(exHash, 0, 4);

            input = $"__fallback__:{obj.GetType().FullName}:{shortExHash}";
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = ComputeSha256Hash(bytes);

        return ToHexString(hash, 0, 8);
    }

#if NET5_0_OR_GREATER
    private static byte[] ComputeSha256Hash(byte[] input)
    {
        return SHA256.HashData(input);
    }

    private static string ToHexString(byte[] bytes, int offset, int length)
    {
        return Convert.ToHexString(bytes, offset, length);
    }
#else
    private static byte[] ComputeSha256Hash(byte[] input)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(input);
    }

    private static string ToHexString(byte[] bytes, int offset, int length)
    {
        var sb = new StringBuilder(length * 2);
        for (int i = offset; i < offset + length; i++)
        {
            sb.Append(bytes[i].ToString("X2"));
        }
        return sb.ToString();
    }
#endif
}

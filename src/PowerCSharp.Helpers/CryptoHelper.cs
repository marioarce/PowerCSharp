using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PowerCSharp.Helpers;

/// <summary>
/// Helper class for cryptography operations
/// </summary>
public static class CryptoHelper
{
    /// <summary>
    /// Computes SHA256 hash of a string
    /// </summary>
    public static string ComputeSHA256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(bytes);

        var result = Convert.ToBase64String(hash);
        return result;
    }

    /// <summary>
    /// Computes MD5 hash of a string
    /// </summary>
    public static string ComputeMD5(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(bytes);

        var result = BitConverter
            .ToString(hash)
            .Replace("-", "")
            .ToLowerInvariant();

        return result;
    }

    /// <summary>
    /// Generates a random string of specified length
    /// </summary>
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var random = new Random();

        var result = new string(
            Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray()
        );

        return result;
    }
}

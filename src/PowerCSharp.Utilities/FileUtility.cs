using System;
using System.IO;

namespace PowerCSharp.Utilities;

/// <summary>
/// Utility class for common file operations
/// </summary>
public static class FileUtility
{
    /// <summary>
    /// Safely reads all text from a file
    /// </summary>
    public static string SafeReadAllText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Safely writes text to a file, creating directory if needed
    /// </summary>
    public static bool SafeWriteAllText(string path, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets file size in human-readable format
    /// </summary>
    public static string GetFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double len = bytes;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

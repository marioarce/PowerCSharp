using System;
using System.IO;

namespace PowerCSharp.Extensions.IO;

#if NET6_0_OR_GREATER
/// <summary>
/// Provides extension methods and static utility methods for directory operations.
/// Contains methods for safely deleting directories with proper error handling and attribute management.
/// </summary>
/// <remarks>
/// This class combines extension methods (for DirectoryInfo instances) and static utility methods (for string paths)
/// to provide comprehensive directory deletion capabilities that handle read-only files and subdirectories.
/// All methods are designed to work with .NET 6.0 and later versions.
/// </remarks>
public static class DirectoryExtensions
{
    /// <summary>
    /// Safely deletes a directory and all of its subdirectories and files.
    /// Clears read-only attributes to prevent exceptions.
    /// Throws exceptions on failure (similar to Directory.Delete but with better error handling).
    /// </summary>
    public static void SafeDelete(this DirectoryInfo directory, bool recursive = true)
    {
        ArgumentNullException.ThrowIfNull(directory);

        if (!directory.Exists)
        {
            return;
        }

        if (recursive)
        {
            // Single traversal to clear attributes
            foreach (var fileSystemInfo in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                if ((fileSystemInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fileSystemInfo.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
        }

        try
        {
            directory.Delete(recursive);
        }
        catch (IOException ex)
        {
            // Log error or rethrow with more context
            throw new IOException($"Failed to delete directory '{directory.FullName}'", ex);
        }
    }

    /// <summary>
    /// Attempts to safely delete a directory and all of its subdirectories and files.
    /// Clears read-only attributes to prevent exceptions.
    /// Never throws exceptions - returns true on success, false on failure.
    /// </summary>
    public static bool TrySafeDelete(this DirectoryInfo directory, bool recursive = true)
    {
        try
        {
            if (directory == null || !directory.Exists)
            {
                return true; // Nothing to delete = success
            }

            if (recursive)
            {
                foreach (var fileSystemInfo in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    fileSystemInfo.Attributes &= ~FileAttributes.ReadOnly;
                }
            }

            directory.Delete(recursive);
            return true;
        }
        catch
        {
            return false; // Swallow all exceptions
        }
    }

    /// <summary>
    /// Attempts to safely delete a directory and all of its subdirectories and files.
    /// Clears read-only attributes to prevent exceptions.
    /// Never throws exceptions - returns true on success, false on failure.
    /// </summary>
    public static bool TrySafeDelete(string path, bool recursive = true)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                return true; // Nothing to delete = success
            }

            var directory = new DirectoryInfo(path);

            return directory.TrySafeDelete(recursive);
        }
        catch
        {
            return false; // Swallow all exceptions
        }
    }
}
#endif

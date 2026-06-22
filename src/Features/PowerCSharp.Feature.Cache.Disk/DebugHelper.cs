using Microsoft.Extensions.Logging;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Helper class for debugging disk cache issues.
/// </summary>
public static class DebugHelper
{
    /// <summary>
    /// Logs a disk cache operation at Debug level, including the key, value, and elapsed time.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="operation">The operation name (e.g. Get, Set, Remove).</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The cached value, if any.</param>
    /// <param name="elapsedMs">The elapsed time for the operation, in milliseconds.</param>
    public static void LogCacheOperation<T>(ILogger logger, string operation, string key, T? value = default, long elapsedMs = 0)
    {
        if (value != null)
        {
            logger.LogDebug("DiskCache {Operation} - Key: {Key}, Value: {Value}, Elapsed: {Elapsed}ms",
                operation, key, value, elapsedMs);
        }
        else
        {
            logger.LogDebug("DiskCache {Operation} - Key: {Key}, Value: <null>, Elapsed: {Elapsed}ms",
                operation, key, elapsedMs);
        }
    }

    /// <summary>
    /// Logs a dependency injection registration status at Information level.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="service">The service name being registered.</param>
    /// <param name="status">The registration status (e.g. Registered, Skipped).</param>
    public static void LogDependencyInjection(ILogger logger, string service, string status)
    {
        logger.LogInformation("DI Debug - Service: {Service}, Status: {Status}", service, status);
    }
}

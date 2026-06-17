using Microsoft.Extensions.Logging;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// Helper class for debugging disk cache issues.
/// </summary>
public static class DebugHelper
{
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

    public static void LogDependencyInjection(ILogger logger, string service, string status)
    {
        logger.LogInformation("DI Debug - Service: {Service}, Status: {Status}", service, status);
    }
}

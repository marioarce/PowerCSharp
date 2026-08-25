using Microsoft.Extensions.Logging;

namespace PowerCSharp.Operational.EventLog;

/// <summary>
/// Cleans up old disk event-log files based on a retention policy, so <see cref="EventLogWriter"/>
/// output doesn't grow disk usage unbounded in production.
/// </summary>
public static class EventLogRetentionCleaner
{
    /// <summary>
    /// Runs retention cleanup: deletes date-partitioned log directories older than
    /// <paramref name="retentionDays"/>. Runs on a background task and never throws.
    /// </summary>
    /// <param name="basePath">The base directory where logs are stored.</param>
    /// <param name="appName">The application name used in the log directory structure.</param>
    /// <param name="retentionDays">The number of days to retain log files.</param>
    /// <param name="logger">An optional logger used to record cleanup failures.</param>
    public static void RunRetentionCleanup(string? basePath, string? appName, int retentionDays, ILogger? logger = null)
    {
        if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(appName))
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                var rootPath = Path.Combine(basePath, appName);
                if (!Directory.Exists(rootPath))
                {
                    return;
                }

                var now = DateTime.UtcNow.Date;

                foreach (var yearDir in Directory.EnumerateDirectories(rootPath))
                {
                    if (!int.TryParse(Path.GetFileName(yearDir), out var year))
                    {
                        continue;
                    }

                    foreach (var monthDir in Directory.EnumerateDirectories(yearDir))
                    {
                        if (!int.TryParse(Path.GetFileName(monthDir), out var month))
                        {
                            continue;
                        }

                        foreach (var dayDir in Directory.EnumerateDirectories(monthDir))
                        {
                            if (!int.TryParse(Path.GetFileName(dayDir), out var day))
                            {
                                continue;
                            }

                            DateTime dirDate;
                            try
                            {
                                dirDate = new DateTime(year, month, day);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                continue; // Not a valid date-shaped directory — skip rather than throw.
                            }

                            if ((now - dirDate).TotalDays <= retentionDays)
                            {
                                continue;
                            }

                            try
                            {
                                Directory.Delete(dayDir, recursive: true);
                            }
                            catch (IOException) { /* Locked file — retry on the next pass. */ }
                            catch (UnauthorizedAccessException) { /* Permissions issue — retry on the next pass. */ }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Event-log retention cleanup failed.");
            }
        });
    }
}

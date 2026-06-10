#if NET8_0_OR_GREATER
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// IHostedService wrapper for the disk cache background timer. Only compiled on net8.0
/// so the core service remains framework-agnostic. The host starts/stops cleanup with graceful shutdown.
/// </summary>
internal sealed class DiskCacheBackgroundService : IHostedService, IDisposable
{
    private readonly DiskCacheService _cache;
    private readonly IOptions<DiskCacheOptions> _options;
    private readonly ILogger<DiskCacheBackgroundService> _logger;
    private System.Threading.Timer? _cleanupTimer;

    public DiskCacheBackgroundService(DiskCacheService cache, IOptions<DiskCacheOptions> options, ILogger<DiskCacheBackgroundService> logger)
    {
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Value.EnableBackgroundCleanup)
        {
            var interval = TimeSpan.FromSeconds(_options.Value.CleanupIntervalSeconds);
            _cleanupTimer = new System.Threading.Timer(
                _ => _cache.PurgeExpired(),
                null,
                interval,
                interval);
            _logger.LogInformation("Disk cache background cleanup started (interval: {Interval}s)", _options.Value.CleanupIntervalSeconds);
        }
        else
        {
            _logger.LogInformation("Disk cache background cleanup disabled");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cleanupTimer?.Dispose();
        _logger.LogInformation("Disk cache background cleanup stopped");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
#endif

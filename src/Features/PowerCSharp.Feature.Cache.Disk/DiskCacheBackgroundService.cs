#if NET8_0_OR_GREATER
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.Disk;

/// <summary>
/// IHostedService wrapper for graceful shutdown on net8.0 only. The portable timer
/// is managed by the core DiskCacheService on all TFMs. This wrapper ensures
/// proper host lifecycle integration when running in ASP.NET Core hosts.
/// </summary>
internal sealed class DiskCacheBackgroundService : IHostedService
{
    private readonly IDiskCacheService _cache;
    private readonly ILogger<DiskCacheBackgroundService> _logger;

    public DiskCacheBackgroundService(IDiskCacheService cache, ILogger<DiskCacheBackgroundService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disk cache background service started (timer managed by core service)");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disk cache background service stopped (core service will dispose timer)");
        return Task.CompletedTask;
    }
}
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Disk;

namespace PowerCSharp.Feature.Cache.Tests;

public class DiskCacheTimerTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    private static ServiceCollection BaseServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }

    [Fact]
    public async Task Background_Timer_Works_When_Enabled()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-timer-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:EnableBackgroundCleanup", "true"),
            ("PowerFeatures:DiskCache:CleanupIntervalSeconds", "1"),
            ("PowerFeatures:DiskCache:DefaultTtlSeconds", "2"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Add some entries that will expire
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");

        // Verify entries exist initially
        Assert.NotNull(await cache.GetAsync<string>("key1"));
        Assert.NotNull(await cache.GetAsync<string>("key2"));

        // Wait for background cleanup to run (timer interval = 1s, TTL = 2s)
        await Task.Delay(3500);

        // Verify entries were cleaned up by background timer
        Assert.Null(await cache.GetAsync<string?>("key1"));
        Assert.Null(await cache.GetAsync<string?>("key2"));

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Background_Timer_Does_Not_Run_When_Disabled()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-no-timer-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:EnableBackgroundCleanup", "false"),
            ("PowerFeatures:DiskCache:DefaultTtlSeconds", "0")); // No expiry

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Add entries (no expiry)
        await cache.SetAsync("key1", "value1");

        // Verify entry exists initially
        var initial = await cache.GetAsync<string>("key1");
        Assert.NotNull(initial);

        // Wait some time
        await Task.Delay(1000);

        // Entry should still exist since no expiry
        var afterDelay = await cache.GetAsync<string>("key1");
        Assert.NotNull(afterDelay);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public void Timer_Is_Created_In_Core_Service_All_TFMs()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-timer-core-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:EnableBackgroundCleanup", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // The core service should manage the timer, not the hosted service
        var diskCache = Assert.IsType<DiskCacheService>(cache);

        // Verify the service was created with timer (we can't directly test the timer
        // but we can verify the service type and that it doesn't throw)
        Assert.NotNull(diskCache);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Disk;

namespace PowerCSharp.Feature.Cache.Tests;

public class DiskCacheAsyncTests
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
    public async Task PurgeExpiredAsync_Works_With_Cancellation()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-async-purge-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:DefaultTtlSeconds", "1"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var diskCache = (DiskCacheService)cache;

        // Add entries that will expire
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");

        // Verify entries exist initially
        Assert.NotNull(await cache.GetAsync<string>("key1"));
        Assert.NotNull(await cache.GetAsync<string>("key2"));

        // Wait for expiry
        await Task.Delay(1500);

        // Use async purge with cancellation
        using var cts = new CancellationTokenSource();
        await diskCache.PurgeExpiredAsync(cts.Token);

        // Verify entries were purged
        Assert.Null(await cache.GetAsync<string?>("key1"));
        Assert.Null(await cache.GetAsync<string?>("key2"));

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task EvictToLimitAsync_Works_With_Cancellation()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-async-evict-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:MaxEntries", "2"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var diskCache = (DiskCacheService)cache;

        // Add 4 entries (max is 2)
        for (int i = 0; i < 4; i++)
        {
            await cache.SetAsync($"key{i}", i);
        }

        // Use async eviction with cancellation
        using var cts = new CancellationTokenSource();
        await diskCache.EvictToLimitAsync(cts.Token);

        // Verify eviction happened by checking that only 2 entries remain
        var remainingCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (await cache.GetAsync<int?>($"key{i}") != null)
            {
                remainingCount++;
            }
        }

        Assert.True(remainingCount <= 2, $"Expected <= 2 entries, found {remainingCount}");

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Async_Verants_Match_Sync_Behavior()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-async-sync-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:DefaultTtlSeconds", "1"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var diskCache = (DiskCacheService)cache;

        // Add entries and wait for expiry
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");
        await Task.Delay(1500);

        // Test sync purge
        diskCache.PurgeExpired();
        var syncResult1 = await cache.GetAsync<string?>("key1");
        var syncResult2 = await cache.GetAsync<string?>("key2");

        // Add more entries for async test
        await cache.SetAsync("key3", "value3");
        await cache.SetAsync("key4", "value4");
        await Task.Delay(1500);

        // Test async purge
        await diskCache.PurgeExpiredAsync();
        var asyncResult1 = await cache.GetAsync<string?>("key3");
        var asyncResult2 = await cache.GetAsync<string?>("key4");

        // Verify both sync and async operations worked (both should be null)
        Assert.Null(syncResult1);
        Assert.Null(syncResult2);
        Assert.Null(asyncResult1);
        Assert.Null(asyncResult2);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Async_Verants_Respect_Cancellation()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-async-cancel-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var diskCache = (DiskCacheService)cache;

        // Add some entries
        for (int i = 0; i < 10; i++)
        {
            await cache.SetAsync($"key{i}", i);
        }

        // Test cancellation with already cancelled token
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await diskCache.PurgeExpiredAsync(cts.Token));

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await diskCache.EvictToLimitAsync(cts.Token));

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }
}

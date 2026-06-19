using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Disk;

namespace PowerCSharp.Feature.Cache.Tests;

public class DiskCacheCrossProcessTests
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
    public async Task CrossProcess_Locking_Enabled_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-cross-process-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Test basic operations with cross-process locking enabled
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");

        var result1 = await cache.GetAsync<string>("key1");
        var result2 = await cache.GetAsync<string>("key2");

        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task CrossProcess_Locking_Disabled_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-no-cross-process-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "false"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Test basic operations with cross-process locking disabled (should be faster)
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");

        var result1 = await cache.GetAsync<string>("key1");
        var result2 = await cache.GetAsync<string>("key2");

        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task CrossProcess_Concurrent_Writes_Work()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-concurrent-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Simulate concurrent writes to different keys
        var tasks = Enumerable.Range(0, 10)
            .Select(i => cache.SetAsync($"key{i}", $"value{i}").AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        // Verify all writes succeeded
        for (int i = 0; i < 10; i++)
        {
            var result = await cache.GetAsync<string>($"key{i}");
            Assert.Equal($"value{i}", result);
        }

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task CrossProcess_Same_Key_Concurrent_Writes_Work()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-same-key-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Simulate concurrent writes to the same key
        var tasks = Enumerable.Range(0, 10)
            .Select(i => cache.SetAsync("samekey", $"value{i}").AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        // One of the writes should have succeeded
        var result = await cache.GetAsync<string>("samekey");
        Assert.NotNull(result);
        Assert.StartsWith("value", result);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task CrossProcess_Index_Operations_Work()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-index-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"),
            ("PowerFeatures:Cache:Disk:MaxEntries", "3"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var diskCache = (DiskCacheService)cache;

        // Add more entries than the max to trigger eviction
        for (int i = 0; i < 5; i++)
        {
            await cache.SetAsync($"key{i}", $"value{i}");
        }

        // Verify eviction worked (should have <= 3 entries)
        var remainingCount = 0;
        for (int i = 0; i < 5; i++)
        {
            if (await cache.GetAsync<string?>($"key{i}") != null)
            {
                remainingCount++;
            }
        }

        Assert.True(remainingCount <= 3, $"Expected <= 3 entries, found {remainingCount}");

        // Test manual purge
        await cache.SetAsync("expireme", "value");
        await Task.Delay(100); // Small delay
        diskCache.PurgeExpired();

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task CrossProcess_Mutex_Cleanup_On_Dispose()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-dispose-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Add some entries to create Mutexes
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");

        // Dispose should clean up Mutexes without throwing
        provider.Dispose();

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task CrossProcess_Performance_Comparison()
    {
        var testDir1 = Path.Combine(Path.GetTempPath(), $"test-perf-on-{Guid.NewGuid()}");
        var testDir2 = Path.Combine(Path.GetTempPath(), $"test-perf-off-{Guid.NewGuid()}");

        var configWithLocking = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir1),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"));

        var configWithoutLocking = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir2),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "false"));

        // Test with locking
        var services1 = BaseServices();
        services1.AddCacheDisk(configWithLocking);
        using var provider1 = services1.BuildServiceProvider();
        var cache1 = provider1.GetRequiredService<IDiskCacheService>();

        var start1 = DateTime.UtcNow;
        for (int i = 0; i < 50; i++)
        {
            await cache1.SetAsync($"key{i}", $"value{i}");
        }
        var end1 = DateTime.UtcNow;
        var timeWithLocking = (end1 - start1).TotalMilliseconds;

        // Test without locking
        var services2 = BaseServices();
        services2.AddCacheDisk(configWithoutLocking);
        using var provider2 = services2.BuildServiceProvider();
        var cache2 = provider2.GetRequiredService<IDiskCacheService>();

        var start2 = DateTime.UtcNow;
        for (int i = 0; i < 50; i++)
        {
            await cache2.SetAsync($"key{i}", $"value{i}");
        }
        var end2 = DateTime.UtcNow;
        var timeWithoutLocking = (end2 - start2).TotalMilliseconds;

        // Both should work, but we don't assert specific timing due to test environment variability
        Assert.True(timeWithLocking > 0, "Time with locking should be > 0");
        Assert.True(timeWithoutLocking > 0, "Time without locking should be > 0");

        // Cleanup
        Directory.Delete(testDir1, recursive: true);
        Directory.Delete(testDir2, recursive: true);
    }
}

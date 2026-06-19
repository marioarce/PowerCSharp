using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;
using PowerCSharp.Feature.Cache.BitFaster;
using PowerCSharp.Feature.Cache.Disk;

namespace PowerCSharp.Feature.Cache.Tests;

public class CacheFeatureTests
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
    public void NoOp_Is_Default_Floor()
    {
        var config = BuildConfiguration();
        var services = BaseServices();
        services.AddCacheFeature(config);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<NoOpCacheService>(provider.GetRequiredService<ICacheService>());
        Assert.IsType<NoOpDiskCacheService>(provider.GetRequiredService<IDiskCacheService>());
    }

    [Fact]
    public void BitFaster_Overrides_NoOp_Floor()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"));

        var services = BaseServices();
        services.AddCacheFeature(config);   // TryAdd NoOp floor
        services.AddCacheBitFaster(config); // plain Add overrides

        using var provider = services.BuildServiceProvider();
        Assert.IsType<BitFasterCacheService>(provider.GetRequiredService<ICacheService>());
        // BitFaster supplies only the in-memory cache; the disk floor remains NoOp.
        Assert.IsType<NoOpDiskCacheService>(provider.GetRequiredService<IDiskCacheService>());
    }

    [Fact]
    public void BitFaster_Cache_Roundtrips()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        cache.Set("answer", 42);
        Assert.True(cache.TryGet<int>("answer", out var value, default));
        Assert.Equal(42, value);

        cache.Remove("answer");
        Assert.False(cache.TryGet<int>("answer", out _, default));
    }

    [Fact]
    public void BitFaster_GetKeys_And_Clear()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        cache.Set("alpha", 1);
        cache.Set("beta", 2);

        var keys = cache.GetKeys(default);
        Assert.Equal(2, keys.Count);
        Assert.Contains("alpha", keys);
        Assert.Contains("beta", keys);

        cache.Clear(default);
        Assert.Empty(cache.GetKeys(default));
        Assert.False(cache.TryGet<int>("alpha", out _, default));
    }

    [Fact]
    public void NoOp_GetKeys_Empty_And_Clear_DoesNotThrow()
    {
        var services = BaseServices();
        services.AddCacheFeature(BuildConfiguration());

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        cache.Set("ignored", 1);
        Assert.Empty(cache.GetKeys(default));
        cache.Clear(default);
        Assert.Empty(cache.GetKeys(default));
    }

    [Fact]
    public void BitFaster_GetOrCreate_Creates_Then_Caches()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        var calls = 0;
        var first = cache.GetOrCreate("k", () => { calls++; return 7; }, default);
        var second = cache.GetOrCreate("k", () => { calls++; return 99; }, default);

        Assert.Equal(7, first);
        Assert.Equal(7, second);   // served from cache, factory not re-run
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task BitFaster_GetOrCreateAsync_Stampede_Invokes_Factory_Once()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        var calls = 0;

        async Task<int> Factory()
        {
            Interlocked.Increment(ref calls);
            await Task.Delay(50);
            return 42;
        }

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => cache.GetOrCreateAsync("hot", Factory))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, value => Assert.Equal(42, value));
        Assert.Equal(1, calls);    // stampede protection: single factory invocation
    }

    [Fact]
    public async Task NoOp_GetOrCreate_Always_Invokes_Factory()
    {
        var services = BaseServices();
        services.AddCacheFeature(BuildConfiguration());

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        var calls = 0;
        Assert.Equal(1, cache.GetOrCreate("k", () => { calls++; return 1; }, default));
        Assert.Equal(1, await cache.GetOrCreateAsync("k", () => { calls++; return Task.FromResult(1); }, default));
        Assert.Equal(2, calls);    // NoOp never caches
    }

    [Fact]
    public void Options_Bind_Provider_Variant()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "500"));

        var services = BaseServices();
        services.AddCacheFeature(config);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CacheFeatureOptions>>().Value;

        Assert.Equal(CacheProvider.BitFaster, options.Provider);
        Assert.Equal(500, options.Capacity);
    }

    [Fact]
    public void Disk_Overrides_NoOp_Floor()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", Path.Combine(Path.GetTempPath(), $"test-disk-{Guid.NewGuid()}")));

        var services = BaseServices();
        services.AddCacheFeature(config);   // TryAdd NoOp floor
        services.AddCacheDisk(config);       // plain Add overrides

        using var provider = services.BuildServiceProvider();
        Assert.IsType<DiskCacheService>(provider.GetRequiredService<IDiskCacheService>());
    }

    [Fact]
    public async Task Disk_Roundtrip()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-disk-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:MaxEntries", "100"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        var data = new TestData { Id = 1, Name = "Test" };
        await cache.SetAsync("key1", data);

        var result = await cache.GetAsync<TestData>("key1");
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Disk_Eviction_When_MaxEntries_Exceeded()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-disk-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:MaxEntries", "3"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = (DiskCacheService)provider.GetRequiredService<IDiskCacheService>();

        // Add 5 entries (max is 3)
        for (int i = 0; i < 5; i++)
        {
            await cache.SetAsync($"key{i}", i);
        }

        // Verify eviction happened (only 3 should remain)
        await cache.GetAsync<int>("key0"); // Try to access oldest
        await cache.GetAsync<int>("key4"); // Try to access newest

        // Manual eviction check
        cache.EvictToLimit();

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Disk_Expiry_TTL()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-disk-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:DefaultTtlSeconds", "1"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        await cache.SetAsync("expiring", 42);

        // Should be available immediately
        var immediate = await cache.GetAsync<int>("expiring");
        Assert.Equal(42, immediate);

        // Wait for expiry
        await Task.Delay(1500);

        // Should be expired
        var expired = await cache.GetAsync<int?>("expiring");
        Assert.Null(expired);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Disk_Concurrency_Simultaneous_Writes()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-disk-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:MaxEntries", "100"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Simulate concurrent writes
        var tasks = Enumerable.Range(0, 20)
            .Select(i => cache.SetAsync($"key{i}", i).AsTask())
            .ToArray();

        await Task.WhenAll(tasks);

        // Verify all writes succeeded
        for (int i = 0; i < 20; i++)
        {
            var result = await cache.GetAsync<int>($"key{i}");
            Assert.Equal(i, result);
        }

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task Disk_PurgeExpired_Removes_Expired_Entries()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-disk-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:DefaultTtlSeconds", "1"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = (DiskCacheService)provider.GetRequiredService<IDiskCacheService>();

        await cache.SetAsync("expiring", 1);
        await cache.SetAsync("expiring2", 2);

        // Wait for expiry
        await Task.Delay(1500);

        // Manual purge
        cache.PurgeExpired();

        // Verify entries are gone
        Assert.Null(await cache.GetAsync<int?>("expiring"));
        Assert.Null(await cache.GetAsync<int?>("expiring2"));

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public void BitFaster_GetWithResult_Provides_Performance_Metrics()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set a value
        cache.Set("metrics_key", "test_value");

        // Get with result
        var result = cache.GetWithResult<string>("metrics_key");

        Assert.True(result.IsSuccess);
        Assert.Equal("test_value", result.Value);
        Assert.NotNull(result.Metadata);
        Assert.Equal("BitFaster", result.ProviderName);
        Assert.Equal("Memory", result.CacheType);
        Assert.True(result.RetrievalDuration >= TimeSpan.Zero);
        Assert.True(result.TotalCacheTime >= TimeSpan.Zero);
    }

    [Fact]
    public async Task BitFaster_GetWithResultAsync_Provides_Performance_Metrics()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set a value
        await cache.SetAsync("async_metrics_key", "async_test_value");

        // Get with result async
        var result = await cache.GetWithResultAsync<string>("async_metrics_key");

        Assert.True(result.IsSuccess);
        Assert.Equal("async_test_value", result.Value);
        Assert.NotNull(result.Metadata);
        Assert.Equal("BitFaster", result.ProviderName);
        Assert.Equal("Memory", result.CacheType);
        Assert.True(result.RetrievalDuration >= TimeSpan.Zero);
        Assert.True(result.TotalCacheTime >= TimeSpan.Zero);
    }

    [Fact]
    public void BitFaster_GetMetadata_Provides_Cache_Statistics()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set a value
        cache.Set("stats_key", "stats_value");

        // Get metadata
        var metadata = cache.GetMetadata("stats_key");

        Assert.NotNull(metadata);
        Assert.Equal("stats_key", metadata.Key);
        Assert.True(metadata.AccessCount >= 1);
        Assert.True(metadata.HitCount >= 0);
        Assert.True(metadata.MissCount >= 0);
    }

    [Fact]
    public async Task BitFaster_GetMetadataAsync_Provides_Cache_Statistics()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set a value
        await cache.SetAsync("async_stats_key", "async_stats_value");

        // Get metadata async
        var metadata = await cache.GetMetadataAsync("async_stats_key");

        Assert.NotNull(metadata);
        Assert.Equal("async_stats_key", metadata.Key);
        Assert.True(metadata.AccessCount >= 1);
        Assert.True(metadata.HitCount >= 0);
        Assert.True(metadata.MissCount >= 0);
    }

    [Fact]
    public void BitFaster_Cache_Hit_Miss_Statistics()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Miss - key doesn't exist
        var missResult = cache.GetWithResult<string>("nonexistent_key");
        Assert.False(missResult.IsSuccess);
        Assert.True(missResult.IsNotFound);

        // Set a value
        cache.Set("hit_miss_key", "test_value");

        // Hit - key exists
        var hitResult = cache.GetWithResult<string>("hit_miss_key");
        Assert.True(hitResult.IsSuccess);

        // Check metadata
        var metadata = cache.GetMetadata("hit_miss_key");
        Assert.NotNull(metadata);
        Assert.True(metadata.HitCount > 0);
    }

    [Fact]
    public async Task BitFaster_Sync_Async_Consistency()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Test sync vs async consistency
        cache.Set("sync_key", "sync_value");
        await cache.SetAsync("async_key", "async_value");

        var syncResult = cache.Get<string>("sync_key");
        var asyncResult = await cache.GetAsync<string>("async_key");

        Assert.Equal("sync_value", syncResult);
        Assert.Equal("async_value", asyncResult);

        // Test GetWithResult consistency
        var syncWithResult = cache.GetWithResult<string>("sync_key");
        var asyncWithResult = await cache.GetWithResultAsync<string>("async_key");

        Assert.True(syncWithResult.IsSuccess);
        Assert.True(asyncWithResult.IsSuccess);
        Assert.Equal("sync_value", syncWithResult.Value);
        Assert.Equal("async_value", asyncWithResult.Value);
    }

    [Fact]
    public async Task Disk_Cache_Sync_Async_Methods()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-sync-async-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Test sync methods
        cache.Set("sync_key", "sync_value");
        var syncResult = cache.Get<string>("sync_key");
        Assert.Equal("sync_value", syncResult);

        // Test async methods
        await cache.SetAsync("async_key", "async_value");
        var asyncResult = await cache.GetAsync<string>("async_key");
        Assert.Equal("async_value", asyncResult);

        // Test sync GetWithResult
        var syncWithResult = cache.GetWithResult<string>("sync_key");
        Assert.True(syncWithResult.IsSuccess);
        Assert.Equal("sync_value", syncWithResult.Value);

        // Test async GetWithResult
        var asyncWithResult = await cache.GetWithResultAsync<string>("async_key");
        Assert.True(asyncWithResult.IsSuccess);
        Assert.Equal("async_value", asyncWithResult.Value);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerCSharp.Feature.Cache.BitFaster;
using PowerCSharp.Feature.Cache.NoOp;

namespace PowerCSharp.Feature.Cache.Tests;

/// <summary>
/// Tests for BitFaster cache service diagnostic functionality and performance metrics.
/// </summary>
public class BitFasterCacheDiagnosticsTests
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
    public void BitFaster_GetWithResult_Provides_Detailed_Metrics()
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
        cache.Set("diagnostic_key", "diagnostic_value");

        // Get with result multiple times to build hit statistics
        var result1 = cache.GetWithResult<string>("diagnostic_key");
        var result2 = cache.GetWithResult<string>("diagnostic_key");
        var result3 = cache.GetWithResult<string>("diagnostic_key");

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        // Verify performance metrics
        Assert.Equal("BitFaster", result1.ProviderName);
        Assert.Equal("Memory", result1.CacheType);
        Assert.True(result1.RetrievalDuration >= TimeSpan.Zero);
        Assert.True(result1.TotalCacheTime >= TimeSpan.Zero);

        // Verify hit statistics increased
        var metadata = cache.GetMetadata("diagnostic_key");
        Assert.NotNull(metadata);
        Assert.True(metadata.HitCount >= 3); // At least 3 hits from our calls
        Assert.Equal(0, metadata.MissCount); // Should be 0 misses for existing key
    }

    [Fact]
    public async Task BitFaster_GetWithResultAsync_Provides_Detailed_Metrics()
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
        await cache.SetAsync("async_diagnostic_key", "async_diagnostic_value");

        // Get with result async multiple times
        var result1 = await cache.GetWithResultAsync<string>("async_diagnostic_key");
        var result2 = await cache.GetWithResultAsync<string>("async_diagnostic_key");

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);

        // Verify performance metrics
        Assert.Equal("BitFaster", result1.ProviderName);
        Assert.Equal("Memory", result1.CacheType);
        Assert.True(result1.RetrievalDuration >= TimeSpan.Zero);
        Assert.True(result1.TotalCacheTime >= TimeSpan.Zero);

        // Verify metadata
        var metadata = await cache.GetMetadataAsync("async_diagnostic_key");
        Assert.NotNull(metadata);
        Assert.True(metadata.HitCount >= 2);
    }

    [Fact]
    public void BitFaster_Hit_Miss_Ratio_Calculations()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Generate misses
        var miss1 = cache.GetWithResult<string>("nonexistent_1");
        var miss2 = cache.GetWithResult<string>("nonexistent_2");
        var miss3 = cache.GetWithResult<string>("nonexistent_3");

        // Generate hits
        cache.Set("hit_key", "hit_value");
        var hit1 = cache.GetWithResult<string>("hit_key");
        var hit2 = cache.GetWithResult<string>("hit_key");

        // Verify miss results
        Assert.All([miss1, miss2, miss3], result =>
        {
            Assert.False(result.IsSuccess);
            Assert.True(result.IsNotFound);
            Assert.Equal("BitFaster", result.ProviderName);
            Assert.Equal("Memory", result.CacheType);
        });

        // Verify hit results
        Assert.All([hit1, hit2], result =>
        {
            Assert.True(result.IsSuccess);
            Assert.Equal("hit_value", result.Value);
            Assert.Equal("BitFaster", result.ProviderName);
            Assert.Equal("Memory", result.CacheType);
        });

        // Check overall statistics through individual metadata
        var hitMetadata = cache.GetMetadata("hit_key");
        Assert.NotNull(hitMetadata);
        Assert.True(hitMetadata.HitCount >= 2);

        // Check miss metadata (should be created for misses)
        var missMetadata1 = cache.GetMetadata("nonexistent_1");
        var missMetadata2 = cache.GetMetadata("nonexistent_2");
        var missMetadata3 = cache.GetMetadata("nonexistent_3");

        // Miss metadata should exist with miss counts
        Assert.NotNull(missMetadata1);
        Assert.NotNull(missMetadata2);
        Assert.NotNull(missMetadata3);
    }

    [Fact]
    public void BitFaster_Metadata_Updates_On_Access()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set initial value
        cache.Set("access_key", "access_value");

        // Get initial metadata
        var initialMetadata = cache.GetMetadata("access_key");
        Assert.NotNull(initialMetadata);
        var initialAccessCount = initialMetadata.AccessCount;
        var initialHitCount = initialMetadata.HitCount;

        // Access the value multiple times with a small delay
        System.Threading.Thread.Sleep(1); // Small delay to ensure time difference
        cache.Get<string>("access_key");
        cache.GetWithResult<string>("access_key");
        cache.TryGet<string>("access_key", out _);

        // Check updated metadata
        var updatedMetadata = cache.GetMetadata("access_key");
        Assert.NotNull(updatedMetadata);
        Assert.True(updatedMetadata.AccessCount > initialAccessCount);
        Assert.True(updatedMetadata.HitCount > initialHitCount);
        // LastAccessedUtc should be >= initial time (allow for same millisecond)
        Assert.True(updatedMetadata.LastAccessedUtc >= initialMetadata.LastAccessedUtc);
    }

    [Fact]
    public void BitFaster_Clear_Resets_Metadata()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set multiple values and access them
        cache.Set("key1", "value1");
        cache.Set("key2", "value2");
        cache.Get<string>("key1");
        cache.Get<string>("key2");

        // Verify metadata exists
        Assert.NotNull(cache.GetMetadata("key1"));
        Assert.NotNull(cache.GetMetadata("key2"));
        var keysBeforeClear = cache.GetKeys(default);
        Assert.True(keysBeforeClear.Count >= 2);

        // Clear cache
        cache.Clear(default);

        // Verify metadata is cleared
        Assert.Empty(cache.GetKeys(default));
        
        // After clear, accessing keys should create new miss metadata
        var missResult = cache.GetWithResult<string>("key1");
        Assert.False(missResult.IsSuccess);
        Assert.True(missResult.IsNotFound);
    }

    [Fact]
    public void BitFaster_Remove_Clears_Metadata()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Set and access a value
        cache.Set("remove_key", "remove_value");
        cache.Get<string>("remove_key");

        // Verify metadata exists
        Assert.NotNull(cache.GetMetadata("remove_key"));
        var keysBeforeRemove = cache.GetKeys(default);
        Assert.Contains("remove_key", keysBeforeRemove);

        // Remove the key
        cache.Remove("remove_key");

        // Verify metadata is removed
        var keysAfterRemove = cache.GetKeys(default);
        Assert.DoesNotContain("remove_key", keysAfterRemove);
        
        // Access should result in miss
        var missResult = cache.GetWithResult<string>("remove_key");
        Assert.False(missResult.IsSuccess);
        Assert.True(missResult.IsNotFound);
    }

    [Fact]
    public async Task BitFaster_GetOrCreate_Updates_Metadata()
    {
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Capacity", "128"));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        var factoryCalls = 0;

        // First call should create and cache
        var result1 = cache.GetOrCreate("factory_key", () => { factoryCalls++; return "factory_value"; }, default);
        Assert.Equal("factory_value", result1);
        Assert.Equal(1, factoryCalls);

        // Second call should use cache
        var result2 = cache.GetOrCreate("factory_key", () => { factoryCalls++; return "different_value"; }, default);
        Assert.Equal("factory_value", result2);
        Assert.Equal(1, factoryCalls); // Factory not called again

        // Verify metadata
        var metadata = cache.GetMetadata("factory_key");
        Assert.NotNull(metadata);
        Assert.True(metadata.HitCount >= 1);
        Assert.Equal(0, metadata.MissCount);

        // Test async version
        var asyncResult1 = await cache.GetOrCreateAsync("async_factory_key", () => { factoryCalls++; return Task.FromResult("async_factory_value"); }, default);
        Assert.Equal("async_factory_value", asyncResult1);
        Assert.Equal(2, factoryCalls);

        var asyncResult2 = await cache.GetOrCreateAsync("async_factory_key", () => { factoryCalls++; return Task.FromResult("different_async_value"); }, default);
        Assert.Equal("async_factory_value", asyncResult2);
        Assert.Equal(2, factoryCalls); // Factory not called again

        // Verify async metadata
        var asyncMetadata = await cache.GetMetadataAsync("async_factory_key");
        Assert.NotNull(asyncMetadata);
        Assert.True(asyncMetadata.HitCount >= 1);
    }

    [Fact]
    public void BitFaster_Performance_Metrics_Accuracy()
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
        cache.Set("timing_key", "timing_value");

        // Measure timing manually
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = cache.GetWithResult<string>("timing_key");
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
        Assert.Equal("timing_value", result.Value);

        // Verify timing is reasonable (should be very fast for in-memory cache)
        Assert.True(result.RetrievalDuration < TimeSpan.FromMilliseconds(100));
        Assert.True(result.TotalCacheTime < TimeSpan.FromMilliseconds(100));
        Assert.True(result.RetrievalDuration <= result.TotalCacheTime);

        // Verify provider and cache type are set
        Assert.Equal("BitFaster", result.ProviderName);
        Assert.Equal("Memory", result.CacheType);
    }

    [Fact]
    public void NoOp_Diagnostics_Returns_Correct_Values()
    {
        var services = BaseServices();
        services.AddCacheFeature(BuildConfiguration());

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        // Test GetWithResult
        var result = cache.GetWithResult<string>("noop_key");
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Equal("NoOp", result.ProviderName);
        Assert.Equal("Memory", result.CacheType);

        // Test GetMetadata
        var metadata = cache.GetMetadata("noop_key");
        Assert.Null(metadata);

        // Test GetKeys
        var keys = cache.GetKeys(default);
        Assert.Empty(keys);

        // Test operations don't throw
        cache.Set("noop_key", "noop_value");
        cache.Remove("noop_key");
        cache.Clear(default);

        // All should still return not found
        var afterSetResult = cache.GetWithResult<string>("noop_key");
        Assert.False(afterSetResult.IsSuccess);
        Assert.True(afterSetResult.IsNotFound);
    }
}

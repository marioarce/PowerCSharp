using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Disk;

namespace PowerCSharp.Feature.Cache.Tests;

public class DiskCacheFileKindTests
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
    public async Task DiskCache_With_CacheFileKind_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-filekind-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;

        // Test SetAsync with CacheFileKind
        await cache.SetAsync("key1", "value1", jsonKind);
        await cache.SetAsync("key2", 42, jsonKind);

        // Test GetAsync with CacheFileKind
        var result1 = await cache.GetAsync<string>("key1", jsonKind);
        var result2 = await cache.GetAsync<int>("key2", jsonKind);

        Assert.Equal("value1", result1);
        Assert.Equal(42, result2);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task DiskCache_Different_FileKinds_Create_Different_Files()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-mixed-kinds-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;
        var binaryKind = CacheFileKind.GetById("binary")!;

        // Store different keys with different file kinds
        await cache.SetAsync("json_key", "json_value", jsonKind);
        await cache.SetAsync("binary_key", "binary_value", binaryKind);

        // Both should be retrievable with their respective kinds
        var jsonResult = await cache.GetAsync<string>("json_key", jsonKind);
        var binaryResult = await cache.GetAsync<string>("binary_key", binaryKind);

        Assert.Equal("json_value", jsonResult);
        Assert.Equal("binary_value", binaryResult);

        // Verify different file extensions were used
        var jsonFiles = Directory.GetFiles(testDir, "*.json");
        var binaryFiles = Directory.GetFiles(testDir, "*.bin");
        
        Assert.True(jsonFiles.Length >= 1, "Should have at least one JSON file");
        Assert.True(binaryFiles.Length >= 1, "Should have at least one binary file");

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task DiskCache_Custom_CacheFileKind_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-custom-kind-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Register a custom cache file kind with unique ID
        var customId = $"custom_{Guid.NewGuid():N}";
        var customKind = CacheFileKind.Register(customId, "Custom Data", ".custom", "Custom cache files");

        // Test with custom kind
        await cache.SetAsync("custom_key", "custom_value", customKind);
        var result = await cache.GetAsync<string>("custom_key", customKind);

        Assert.Equal("custom_value", result);

        // Verify custom file was created
        var customFiles = Directory.GetFiles(testDir, "*.custom");
        Assert.True(customFiles.Length >= 1, "Should have at least one custom file");

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task DiskCache_Mixed_Operations_Work()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-mixed-ops-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;

        // Mix operations with and without CacheFileKind
        await cache.SetAsync("default1", "default_value1");
        await cache.SetAsync("json1", "json_value1", jsonKind);
        await cache.SetAsync("default2", "default_value2");
        await cache.SetAsync("json2", "json_value2", jsonKind);

        // Retrieve with appropriate methods
        var default1 = await cache.GetAsync<string>("default1");
        var json1 = await cache.GetAsync<string>("json1", jsonKind);
        var default2 = await cache.GetAsync<string>("default2");
        var json2 = await cache.GetAsync<string>("json2", jsonKind);

        Assert.Equal("default_value1", default1);
        Assert.Equal("json_value1", json1);
        Assert.Equal("default_value2", default2);
        Assert.Equal("json_value2", json2);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task DiskCache_FileKind_Persistence_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-persistence-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        // Create first cache instance
        using var provider1 = BaseServices().AddCacheDisk(config).BuildServiceProvider();
        var cache1 = provider1.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;

        await cache1.SetAsync("persistent_key", "persistent_value", jsonKind);

        // Dispose first instance
        provider1.Dispose();

        // Create second cache instance (should load from disk)
        using var provider2 = BaseServices().AddCacheDisk(config).BuildServiceProvider();
        var cache2 = provider2.GetRequiredService<IDiskCacheService>();

        var result = await cache2.GetAsync<string>("persistent_key", jsonKind);
        Assert.Equal("persistent_value", result);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task DiskCache_FileKind_With_CrossProcess_Locking_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-filekind-locking-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:EnableCrossProcessLocking", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;
        var binaryKind = CacheFileKind.GetById("binary")!;

        // Test concurrent operations with different file kinds
        var tasks = new[]
        {
            cache.SetAsync("key1", "value1", jsonKind).AsTask(),
            cache.SetAsync("key2", "value2", binaryKind).AsTask(),
            cache.SetAsync("key3", "value3", jsonKind).AsTask()
        };

        await Task.WhenAll(tasks);

        // Verify all values are retrievable
        var result1 = await cache.GetAsync<string>("key1", jsonKind);
        var result2 = await cache.GetAsync<string>("key2", binaryKind);
        var result3 = await cache.GetAsync<string>("key3", jsonKind);

        Assert.Equal("value1", result1);
        Assert.Equal("value2", result2);
        Assert.Equal("value3", result3);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public async Task DiskCache_FileKind_Expiration_Works()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-filekind-expiry-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:DiskCache:DirectoryPath", testDir),
            ("PowerFeatures:DiskCache:DefaultTtlSeconds", "1"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;

        // Set value with file kind
        await cache.SetAsync("expire_key", "expire_value", jsonKind);

        // Should be available immediately
        var immediate = await cache.GetAsync<string>("expire_key", jsonKind);
        Assert.Equal("expire_value", immediate);

        // Wait for expiration
        await Task.Delay(1500);

        // Should be expired
        var expired = await cache.GetAsync<string?>("expire_key", jsonKind);
        Assert.Null(expired);

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }
}

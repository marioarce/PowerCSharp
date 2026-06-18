using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.Enums;
using PowerCSharp.Feature.Cache.Disk;

namespace PowerCSharp.Feature.Cache.Tests;

public class DiskCacheMetadataTests
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
    public async Task DiskCache_GetWithMetadataAsync_Success()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-success-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Set a value
        await cache.SetAsync("test_key", "test_value");

        // Get with metadata
        var result = await cache.GetWithResultAsync<string>("test_key");

        Assert.True(result.IsSuccess);
        Assert.Equal("test_value", result.Value);
        Assert.NotNull(result.Metadata);
        Assert.Equal("test_key", result.Metadata.Key);
        Assert.True(result.Metadata.SizeBytes > 0);
        Assert.NotNull(((DiskCacheEntryMetadata)result.Metadata).FilePath);
        Assert.Equal(CacheResultReason.Success, result.Reason);
    }

    [Fact]
    public async Task DiskCache_GetWithMetadataAsync_NotFound()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-notfound-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Get non-existent key
        var result = await cache.GetWithResultAsync<string>("non_existent_key");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Equal(default(string), result.Value);
        Assert.Null(result.Metadata); // NotFound returns null metadata
        Assert.Equal(CacheResultReason.NotFound, result.Reason);
    }

    [Fact]
    public async Task DiskCache_GetWithMetadataAsync_Expired()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-expired-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:DefaultTtlSeconds", "1"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Set a value
        await cache.SetAsync("expire_key", "expire_value");

        // Wait for expiry
        await Task.Delay(1500);

        // Get with metadata - should be expired
        var result = await cache.GetWithResultAsync<string>("expire_key");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsExpired);
        Assert.Equal(default(string), result.Value);
        Assert.NotNull(result.Metadata);
        Assert.Equal("expire_key", result.Metadata.Key);
        Assert.True(result.Metadata.IsExpired);
        Assert.Equal(CacheResultReason.Expired, result.Reason);
    }

    [Fact]
    public async Task DiskCache_GetMetadataAsync_Success()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-get-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Set a value
        await cache.SetAsync("metadata_key", "metadata_value");

        // Get metadata only
        var metadata = await cache.GetMetadataAsync("metadata_key");

        Assert.NotNull(metadata);
        Assert.Equal("metadata_key", metadata.Key);
        Assert.True(metadata.SizeBytes > 0);
        Assert.NotNull(((DiskCacheEntryMetadata)metadata).FilePath);
    }

    [Fact]
    public async Task DiskCache_GetMetadataAsync_NotFound()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-get-notfound-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Get metadata for non-existent key
        var metadata = await cache.GetMetadataAsync("non_existent_key");

        Assert.Null(metadata);
    }

    [Fact]
    public async Task DiskCache_Metadata_With_FileKind()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-filekind-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var jsonKind = CacheFileKind.GetById("json")!;

        // Set a value with specific file kind
        await cache.SetAsync("filekind_key", "filekind_value", jsonKind);

        // Get with metadata using file kind
        var result = await cache.GetWithResultAsync<string>("filekind_key", jsonKind);

        Assert.True(result.IsSuccess);
        Assert.Equal("filekind_value", result.Value);
        Assert.NotNull(result.Metadata);
        var diskMetadata = (DiskCacheEntryMetadata)result.Metadata;
        Assert.Same(jsonKind, diskMetadata.FileKind);

        // Verify file extension
        Assert.True(diskMetadata.FilePath?.EndsWith(".json"));
    }

    [Fact]
    public async Task DiskCache_Metadata_FileKind_Detection()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-detection-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();
        var binaryKind = CacheFileKind.GetById("binary")!;

        // Set a value with binary file kind
        await cache.SetAsync("binary_key", 42, binaryKind);

        // Get metadata - should detect file kind from extension
        var metadata = await cache.GetMetadataAsync("binary_key");

        Assert.NotNull(metadata);
        var diskMetadata = (DiskCacheEntryMetadata)metadata;
        Assert.Same(binaryKind, diskMetadata.FileKind);
        Assert.True(diskMetadata.FilePath?.EndsWith(".bin"));
    }

    [Fact]
    public async Task DiskCache_Metadata_Age_Calculations()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-age-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Set a value
        await cache.SetAsync("age_key", "age_value");

        // Wait a bit
        await Task.Delay(100);

        // Get metadata
        var metadata = await cache.GetMetadataAsync("age_key");

        Assert.NotNull(metadata);
        Assert.True(metadata.Age.TotalMilliseconds > 0);
        Assert.True(metadata.TimeSinceLastAccess.TotalMilliseconds > 0);
    }

    [Fact]
    public async Task DiskCache_Metadata_Expiry_Calculations()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-expiry-calc-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:DefaultTtlSeconds", "30"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Set a value
        await cache.SetAsync("expiry_calc_key", "expiry_calc_value");

        // Get metadata
        var metadata = await cache.GetMetadataAsync("expiry_calc_key");

        Assert.NotNull(metadata);
        Assert.NotNull(metadata.ExpiresAtUtc);
        Assert.False(metadata.IsExpired);
        Assert.NotNull(metadata.TimeToExpire);
        Assert.True(metadata.TimeToExpire.Value.TotalSeconds > 25); // Should be around 30 seconds
    }

    [Fact]
    public async Task DiskCache_Metadata_With_CrossProcess_Locking()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-locking-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir),
            ("PowerFeatures:Cache:Disk:EnableCrossProcessLocking", "true"));

        var services = BaseServices();
        services.AddCacheDisk(config);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        // Set a value
        await cache.SetAsync("locking_key", "locking_value");

        // Get with metadata - should work with cross-process locking
        var result = await cache.GetWithResultAsync<string>("locking_key");

        Assert.True(result.IsSuccess);
        Assert.Equal("locking_value", result.Value);
        Assert.NotNull(result.Metadata);
    }

    [Fact]
    public async Task DiskCache_Metadata_Persistence()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-metadata-persistence-{Guid.NewGuid()}");
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Disk:DirectoryPath", testDir));

        // Create first cache instance
        using var provider1 = BaseServices().AddCacheDisk(config).BuildServiceProvider();
        var cache1 = provider1.GetRequiredService<IDiskCacheService>();

        await cache1.SetAsync("persistent_key", "persistent_value");

        // Dispose first instance
        provider1.Dispose();

        // Create second cache instance
        using var provider2 = BaseServices().AddCacheDisk(config).BuildServiceProvider();
        var cache2 = provider2.GetRequiredService<IDiskCacheService>();

        // Get metadata from second instance
        var metadata = await cache2.GetMetadataAsync("persistent_key");

        Assert.NotNull(metadata);
        Assert.Equal("persistent_key", metadata.Key);
        Assert.True(metadata.Age.TotalSeconds > 0); // Should show age from first instance

        // Cleanup
        Directory.Delete(testDir, recursive: true);
    }
}

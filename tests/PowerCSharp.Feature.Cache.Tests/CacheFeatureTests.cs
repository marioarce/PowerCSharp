using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache;
using PowerCSharp.Feature.Cache.BitFaster;
using PowerCSharp.Feature.Cache.NoOp;
using Xunit;

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
        Assert.IsType<FileDiskCacheService>(provider.GetRequiredService<IDiskCacheService>());
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
        Assert.True(cache.TryGet<int>("answer", out var value));
        Assert.Equal(42, value);

        cache.Remove("answer");
        Assert.False(cache.TryGet<int>("answer", out _));
    }

    [Fact]
    public async Task FileDiskCache_Roundtrips()
    {
        var dir = Path.Combine(Path.GetTempPath(), "powercsharp-cache-tests", Guid.NewGuid().ToString("N"));
        var config = BuildConfiguration(
            ("PowerFeatures:Cache:Enabled", "true"),
            ("PowerFeatures:Cache:Provider", "BitFaster"),
            ("PowerFeatures:Cache:Disk:DirectoryPath", dir));

        var services = BaseServices();
        services.AddCacheBitFaster(config);

        using var provider = services.BuildServiceProvider();
        var disk = provider.GetRequiredService<IDiskCacheService>();

        await disk.SetAsync("user", "alice");
        var result = await disk.GetAsync<string>("user");

        Assert.Equal("alice", result);

        Directory.Delete(dir, recursive: true);
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

        var keys = cache.GetKeys();
        Assert.Equal(2, keys.Count);
        Assert.Contains("alpha", keys);
        Assert.Contains("beta", keys);

        cache.Clear();
        Assert.Empty(cache.GetKeys());
        Assert.False(cache.TryGet<int>("alpha", out _));
    }

    [Fact]
    public void NoOp_GetKeys_Empty_And_Clear_DoesNotThrow()
    {
        var services = BaseServices();
        services.AddCacheFeature(BuildConfiguration());

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICacheService>();

        cache.Set("ignored", 1);
        Assert.Empty(cache.GetKeys());
        cache.Clear();
        Assert.Empty(cache.GetKeys());
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
}

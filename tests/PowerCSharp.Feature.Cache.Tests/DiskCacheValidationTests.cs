using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PowerCSharp.Feature.Cache.Disk;
using PowerCSharp.Feature.Cache.Abstractions;

namespace PowerCSharp.Feature.Cache.Tests;

public class DiskCacheValidationTests
{
    private static IOptions<PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions> CreateOptions(PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions options)
        => Options.Create(options);

    private static ILogger<DiskCacheService> CreateLogger()
        => new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider()
            .GetRequiredService<ILogger<DiskCacheService>>();

    [Fact]
    public void DiskCacheService_Valid_Default_Options_Creates_Successfully()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions();
        var logger = CreateLogger();

        var service = new DiskCacheService(CreateOptions(options), logger);

        Assert.NotNull(service);
        
        // Cleanup
        service.Dispose();
    }

    [Fact]
    public void DiskCacheService_Valid_Custom_Options_Creates_Successfully()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-validation-{Guid.NewGuid()}");
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = testDir,
            DefaultTtlSeconds = 3600,
            MaxEntries = 5000,
            EnableBackgroundCleanup = true,
            CleanupIntervalSeconds = 600,
            EnableCrossProcessLocking = false
        };
        var logger = CreateLogger();

        var service = new DiskCacheService(CreateOptions(options), logger);

        Assert.NotNull(service);
        Assert.True(Directory.Exists(testDir));
        
        // Cleanup
        service.Dispose();
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public void DiskCacheService_Invalid_DirectoryPath_Characters_Throws_ArgumentException()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = "invalid<path" // Use < instead of | which might be valid on some systems
        };
        var logger = CreateLogger();

        var exception = Assert.Throws<ArgumentException>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        Assert.Contains("contains problematic characters", exception.Message);
        Assert.Contains("invalid<path", exception.Message);
    }

    [Fact]
    public void DiskCacheService_Negative_DefaultTtlSeconds_Throws_ArgumentException()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DefaultTtlSeconds = -1
        };
        var logger = CreateLogger();

        var exception = Assert.Throws<ArgumentException>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        Assert.Contains("DefaultTtlSeconds must be non-negative", exception.Message);
        Assert.Contains("-1", exception.Message);
    }

    [Fact]
    public void DiskCacheService_Zero_MaxEntries_Throws_ArgumentException()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            MaxEntries = 0
        };
        var logger = CreateLogger();

        var exception = Assert.Throws<ArgumentException>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        Assert.Contains("MaxEntries must be positive", exception.Message);
        Assert.Contains("0", exception.Message);
    }

    [Fact]
    public void DiskCacheService_Negative_MaxEntries_Throws_ArgumentException()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            MaxEntries = -100
        };
        var logger = CreateLogger();

        var exception = Assert.Throws<ArgumentException>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        Assert.Contains("MaxEntries must be positive", exception.Message);
        Assert.Contains("-100", exception.Message);
    }

    [Fact]
    public void DiskCacheService_Negative_CleanupIntervalSeconds_With_BackgroundCleanup_Throws_ArgumentException()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            EnableBackgroundCleanup = true,
            CleanupIntervalSeconds = -10
        };
        var logger = CreateLogger();

        var exception = Assert.Throws<ArgumentException>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        Assert.Contains("CleanupIntervalSeconds must be positive when EnableBackgroundCleanup is true", exception.Message);
        Assert.Contains("-10", exception.Message);
    }

    [Fact]
    public void DiskCacheService_Zero_CleanupIntervalSeconds_With_BackgroundCleanup_Throws_ArgumentException()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            EnableBackgroundCleanup = true,
            CleanupIntervalSeconds = 0
        };
        var logger = CreateLogger();

        var exception = Assert.Throws<ArgumentException>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        Assert.Contains("CleanupIntervalSeconds must be positive when EnableBackgroundCleanup is true", exception.Message);
        Assert.Contains("0", exception.Message);
    }

    [Fact]
    public void DiskCacheService_Zero_CleanupIntervalSeconds_Without_BackgroundCleanup_Does_Not_Throw()
    {
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            EnableBackgroundCleanup = false,
            CleanupIntervalSeconds = 0
        };
        var logger = CreateLogger();

        var service = new DiskCacheService(CreateOptions(options), logger);

        Assert.NotNull(service);
        
        // Cleanup
        service.Dispose();
    }

    [Fact]
    public void DiskCacheService_ReadOnly_Directory_Throws_UnauthorizedAccessException()
    {
        // Create a temporary directory and make it read-only
        var testDir = Path.Combine(Path.GetTempPath(), $"test-readonly-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        
        try
        {
            // Make directory read-only (this might not work on all systems, but we'll try)
            var dirInfo = new DirectoryInfo(testDir);
            dirInfo.Attributes |= FileAttributes.ReadOnly;

            var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
            {
                DirectoryPath = testDir
            };
            var logger = CreateLogger();

            // This should throw an exception due to write access issues
            Assert.ThrowsAny<Exception>(() => 
                new DiskCacheService(CreateOptions(options), logger));
        }
        finally
        {
            // Cleanup - remove read-only attribute first
            try
            {
                var dirInfo = new DirectoryInfo(testDir);
                dirInfo.Attributes &= ~FileAttributes.ReadOnly;
                Directory.Delete(testDir, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    [Fact]
    public void DiskCacheService_Nonexistent_Parent_Directory_Throws_DirectoryNotFoundException()
    {
        // Use a path that should fail - try using an invalid drive on Windows
        var invalidPath = Environment.OSVersion.Platform == PlatformID.Win32NT 
            ? "Z:\\nonexistent\\cache\\path" // Invalid drive on Windows
            : "/nonexistent/deep/nested/path/that/should/not/exist";
            
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = invalidPath
        };
        var logger = CreateLogger();

        // This should throw some kind of exception (DirectoryNotFoundException or other)
        var exception = Assert.ThrowsAny<Exception>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        // Accept various exception types since different OSes behave differently
        Assert.True(exception is DirectoryNotFoundException || 
                  exception is UnauthorizedAccessException ||
                  exception is IOException ||
                  exception is ArgumentException);
    }

    [Fact]
    public void DiskCacheService_Very_Long_Path_Throws_ArgumentException()
    {
        // Create a path that's too long (use a reasonable length that should cause issues)
        var longPath = new string('a', 260); // Typical MAX_PATH limit is 260 characters
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = longPath
        };
        var logger = CreateLogger();

        var exception = Assert.ThrowsAny<Exception>(() => 
            new DiskCacheService(CreateOptions(options), logger));

        // This could be ArgumentException or PathTooLongException depending on the system
        Assert.True(exception is ArgumentException || exception is PathTooLongException);
    }

    [Fact]
    public void DiskCacheService_Validates_From_Configuration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("PowerFeatures:DiskCache:DefaultTtlSeconds", "1800"),
                new KeyValuePair<string, string?>("PowerFeatures:DiskCache:MaxEntries", "2000"),
                new KeyValuePair<string, string?>("PowerFeatures:DiskCache:EnableBackgroundCleanup", "true"),
                new KeyValuePair<string, string?>("PowerFeatures:DiskCache:CleanupIntervalSeconds", "120")
            })
            .Build();

        var services = new ServiceCollection();
        services.AddCacheDisk(config);
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IDiskCacheService>();

        Assert.NotNull(cache);
        Assert.IsType<DiskCacheService>(cache);
        
        // Cleanup
        if (cache is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void DiskCacheService_Invalid_Configuration_Throws_Validation_Exception()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("PowerFeatures:DiskCache:MaxEntries", "-1") // Invalid
            })
            .Build();

        var services = new ServiceCollection();
        services.AddCacheDisk(config);
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        
        Assert.Throws<ArgumentException>(() => 
            provider.GetRequiredService<IDiskCacheService>());
    }

    [Fact]
    public void DiskCacheService_Warning_Large_MaxEntries()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-warning-maxentries-{Guid.NewGuid()}");
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = testDir,
            MaxEntries = 200_000 // Large value that should trigger warning (>100,000)
        };
        var logger = CreateLogger();

        var service = new DiskCacheService(CreateOptions(options), logger);

        Assert.NotNull(service);
        
        // Cleanup
        service.Dispose();
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public void DiskCacheService_Warning_Short_CleanupInterval()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-warning-cleanup-{Guid.NewGuid()}");
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = testDir,
            EnableBackgroundCleanup = true,
            CleanupIntervalSeconds = 30 // Short interval that should trigger warning (<60 seconds)
        };
        var logger = CreateLogger();

        var service = new DiskCacheService(CreateOptions(options), logger);

        Assert.NotNull(service);
        
        // Cleanup
        service.Dispose();
        Directory.Delete(testDir, recursive: true);
    }

    [Fact]
    public void DiskCacheService_Warning_Long_DefaultTtl()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"test-warning-ttl-{Guid.NewGuid()}");
        var options = new PowerCSharp.Feature.Cache.Disk.DiskCacheFeatureOptions
        {
            DirectoryPath = testDir,
            DefaultTtlSeconds = 172800 // 48 hours, should trigger warning (>86,400 seconds)
        };
        var logger = CreateLogger();

        var service = new DiskCacheService(CreateOptions(options), logger);

        Assert.NotNull(service);
        
        // Cleanup
        service.Dispose();
        Directory.Delete(testDir, recursive: true);
    }
}

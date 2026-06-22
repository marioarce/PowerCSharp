using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.Enums;

namespace PowerCSharp.Feature.Cache.Tests;

public class CacheMetadataTests
{
    [Fact]
    public void CacheEntryMetadata_Creates_Correctly()
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(1);
        var diskMetadata = new DiskCacheEntryMetadata(
            "test_key",
            CacheFileKind.GetById("json"),
            now,
            now,
            expiresAt,
            1024,
            "/path/to/file.json");

        Assert.Equal("test_key", diskMetadata.Key);
        Assert.Same(CacheFileKind.GetById("json"), diskMetadata.FileKind);
        Assert.Equal(now, diskMetadata.CreatedAtUtc);
        Assert.Equal(now, diskMetadata.LastAccessedUtc);
        Assert.Equal(expiresAt, diskMetadata.ExpiresAtUtc);
        Assert.Equal(1024, diskMetadata.SizeBytes);
        Assert.Equal("/path/to/file.json", diskMetadata.FilePath);

        // Test in-memory metadata too
        var memoryMetadata = new InMemoryCacheEntryMetadata(
            "memory_key",
            now,
            now,
            expiresAt,
            512,
            false,
            1,
            1,
            0,
            true,
            CacheEntryPriority.Normal,
            0.5);

        Assert.Equal("memory_key", memoryMetadata.Key);
        Assert.Equal(now, memoryMetadata.CreatedAtUtc);
        Assert.Equal(512, memoryMetadata.SizeBytes);
        Assert.Equal(CacheEntryPriority.Normal, memoryMetadata.Priority);
    }

    [Fact]
    public void CacheEntryMetadata_IsExpired_Works()
    {
        var now = DateTime.UtcNow;
        var expiredEntry = new InMemoryCacheEntryMetadata(
            "expired_key",
            now.AddHours(-2),
            now.AddHours(-1),
            now.AddMinutes(-30)); // Expired 30 minutes ago

        var validEntry = new InMemoryCacheEntryMetadata(
            "valid_key",
            now.AddHours(-2),
            now.AddHours(-1),
            now.AddHours(1)); // Expires in 1 hour

        var noExpiryEntry = new InMemoryCacheEntryMetadata(
            "no_expiry_key",
            now.AddHours(-2),
            now.AddHours(-1),
            null); // No expiry

        Assert.True(expiredEntry.IsExpired);
        Assert.False(validEntry.IsExpired);
        Assert.False(noExpiryEntry.IsExpired);
    }

    [Fact]
    public void CacheEntryMetadata_TimeToExpire_Works()
    {
        var now = DateTime.UtcNow;
        var entryWithExpiry = new InMemoryCacheEntryMetadata(
            "key",
            now,
            now,
            now.AddMinutes(30));

        var entryWithoutExpiry = new InMemoryCacheEntryMetadata(
            "key",
            now,
            now,
            null);

        Assert.True(entryWithExpiry.TimeToExpire.HasValue);
        Assert.True(entryWithExpiry.TimeToExpire.Value.TotalMinutes > 25); // Should be around 30 minutes
        Assert.False(entryWithoutExpiry.TimeToExpire.HasValue);
    }

    [Fact]
    public void CacheEntryMetadata_Age_And_TimeSinceLastAccess_Works()
    {
        var now = DateTime.UtcNow;
        var created = now.AddMinutes(-10);
        var lastAccessed = now.AddMinutes(-5);

        var metadata = new InMemoryCacheEntryMetadata(
            "key",
            created,
            lastAccessed,
            null);

        Assert.True(metadata.Age.TotalMinutes >= 9 && metadata.Age.TotalMinutes <= 11); // Around 10 minutes
        Assert.True(metadata.TimeSinceLastAccess.TotalMinutes >= 4 && metadata.TimeSinceLastAccess.TotalMinutes <= 6); // Around 5 minutes
    }

    [Fact]
    public void CacheEntryMetadata_Equals_Works()
    {
        var now = DateTime.UtcNow;
        var metadata1 = new InMemoryCacheEntryMetadata("key", now, now);
        var metadata2 = new InMemoryCacheEntryMetadata("key", now, now);
        var metadata3 = new InMemoryCacheEntryMetadata("different_key", now, now);

        Assert.Equal(metadata1, metadata2);
        Assert.True(metadata1 == metadata2);
        Assert.False(metadata1 != metadata2);

        Assert.NotEqual(metadata1, metadata3);
        Assert.False(metadata1 == metadata3);
        Assert.True(metadata1 != metadata3);
    }

    [Fact]
    public void CacheEntryMetadata_ToString_Works()
    {
        var now = DateTime.UtcNow;
        var diskMetadata = new DiskCacheEntryMetadata(
            "test_key",
            CacheFileKind.GetById("json"),
            now,
            now,
            now.AddMinutes(30),
            1024);

        var diskResult = diskMetadata.ToString();
        Assert.Contains("test_key", diskResult);
        Assert.Contains("active", diskResult);
        Assert.Contains("1024 bytes", diskResult);
        Assert.Contains("expires", diskResult);

        var memoryMetadata = new InMemoryCacheEntryMetadata(
            "memory_key",
            now,
            now,
            now.AddMinutes(30),
            512);

        var memoryResult = memoryMetadata.ToString();
        Assert.Contains("memory_key", memoryResult);
        Assert.Contains("active", memoryResult);
        Assert.Contains("512 bytes", memoryResult);
    }

    [Fact]
    public void CacheResult_Success_Creates_Correctly()
    {
        var metadata = new InMemoryCacheEntryMetadata("key", DateTime.UtcNow, DateTime.UtcNow);
        var result = CacheResult<string>.Success("test_value", metadata);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsNotFound);
        Assert.False(result.IsExpired);
        Assert.Equal("test_value", result.Value);
        Assert.Same(metadata, result.Metadata);
        Assert.Equal(CacheResultReason.Success, result.Reason);
    }

    [Fact]
    public void CacheResult_Success_With_Performance_Metrics()
    {
        var metadata = new InMemoryCacheEntryMetadata("key", DateTime.UtcNow, DateTime.UtcNow);
        var retrievalDuration = TimeSpan.FromMilliseconds(5);
        var totalCacheTime = TimeSpan.FromMilliseconds(8);
        var result = CacheResult<string>.Success("test_value", metadata, "TestProvider", "TestCache", retrievalDuration);

        Assert.True(result.IsSuccess);
        Assert.Equal("test_value", result.Value);
        Assert.Same(metadata, result.Metadata);
        Assert.Equal("TestProvider", result.ProviderName);
        Assert.Equal("TestCache", result.CacheType);
        Assert.Equal(retrievalDuration, result.RetrievalDuration);
        Assert.Equal(retrievalDuration, result.TotalCacheTime); // TotalCacheTime defaults to RetrievalDuration when not specified
    }

    [Fact]
    public void CacheResult_NotFound_Creates_Correctly()
    {
        var result = CacheResult<string>.NotFound("missing_key");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.False(result.IsExpired);
        Assert.Equal(default(string), result.Value);
        Assert.Null(result.Metadata); // NotFound returns null metadata
        Assert.Equal(CacheResultReason.NotFound, result.Reason);
    }

    [Fact]
    public void CacheResult_NotFound_With_Performance_Metrics()
    {
        var retrievalDuration = TimeSpan.FromMilliseconds(2);
        var result = CacheResult<string>.NotFound("missing_key", "TestProvider", "TestCache", retrievalDuration);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Equal(default(string), result.Value);
        Assert.Null(result.Metadata);
        Assert.Equal("TestProvider", result.ProviderName);
        Assert.Equal("TestCache", result.CacheType);
        Assert.Equal(retrievalDuration, result.RetrievalDuration);
    }

    [Fact]
    public void CacheResult_Expired_Creates_Correctly()
    {
        var metadata = new InMemoryCacheEntryMetadata("expired_key", DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-1));
        var result = CacheResult<string>.Expired(metadata);

        Assert.False(result.IsSuccess);
        Assert.False(result.IsNotFound);
        Assert.True(result.IsExpired);
        Assert.Equal(default(string), result.Value);
        Assert.Same(metadata, result.Metadata);
        Assert.Equal(CacheResultReason.Expired, result.Reason);
    }

    [Fact]
    public void CacheResult_Error_Creates_Correctly()
    {
        var result = CacheResult<string>.Error("error_key");

        Assert.False(result.IsSuccess);
        Assert.False(result.IsNotFound);
        Assert.False(result.IsExpired);
        Assert.Equal(default(string), result.Value);
        Assert.Null(result.Metadata); // Error returns null metadata
        Assert.Equal(CacheResultReason.Error, result.Reason);
    }

    [Fact]
    public void CacheResult_GetValueOrDefault_Works()
    {
        var successResult = CacheResult<string>.Success("value");
        var failureResult = CacheResult<string>.NotFound("key");

        Assert.Equal("value", successResult.GetValueOrDefault());
        Assert.Equal("value", successResult.GetValueOrDefault("default"));
        Assert.Equal(default(string), failureResult.GetValueOrDefault());
        Assert.Equal("default", failureResult.GetValueOrDefault("default"));
    }

    [Fact]
    public void CacheResult_GetValueOrThrow_Throws_On_Failure()
    {
        var successResult = CacheResult<string>.Success("value");
        var failureResult = CacheResult<string>.NotFound("key");

        Assert.Equal("value", successResult.GetValueOrThrow());
        Assert.Throws<InvalidOperationException>(() => failureResult.GetValueOrThrow());
    }

    [Fact]
    public void CacheResult_Equals_Works()
    {
        var metadata = new InMemoryCacheEntryMetadata("key", DateTime.UtcNow, DateTime.UtcNow);
        var result1 = CacheResult<string>.Success("value", metadata);
        var result2 = CacheResult<string>.Success("value", metadata);
        var result3 = CacheResult<string>.Success("different_value", metadata);
        var result4 = CacheResult<string>.NotFound("key");

        Assert.Equal(result1, result2);
        Assert.True(result1 == result2);
        Assert.False(result1 != result2);

        Assert.NotEqual(result1, result3);
        Assert.NotEqual(result1, result4);
    }

    [Fact]
    public void CacheResult_ToString_Works()
    {
        var successResult = CacheResult<string>.Success("value");
        var failureResult = CacheResult<string>.NotFound("key");

        var successString = successResult.ToString();
        var failureString = failureResult.ToString();

        Assert.Contains("Success", successString);
        Assert.Contains("String", successString);

        Assert.Contains("NotFound", failureString);
        Assert.Contains("String", failureString);
    }

    [Fact]
    public void CacheResult_ToString_With_Performance_Metrics()
    {
        var metadata = new InMemoryCacheEntryMetadata("key", DateTime.UtcNow, DateTime.UtcNow);
        var result = CacheResult<string>.Success("value", metadata, "TestProvider", "TestCache", TimeSpan.FromMilliseconds(5));

        var resultString = result.ToString();
        Assert.Contains("Success", resultString);
        Assert.Contains("TestProvider", resultString);
        Assert.Contains("TestCache", resultString);
        Assert.Contains("ms", resultString); // Check for milliseconds format
    }

    [Fact]
    public void CacheResult_Hit_Ratio_Calculations()
    {
        var now = DateTime.UtcNow;
        var metadataWithHits = new InMemoryCacheEntryMetadata(
            "key", now, now, null, null, true, 10, 8, 2); // 8 hits, 2 misses = 80% hit ratio
        
        var resultWithHits = CacheResult<string>.Success("value", metadataWithHits);
        Assert.Equal(0.8, resultWithHits.HitRatio);
        Assert.Equal(8, resultWithHits.HitCount);
        Assert.Equal(2, resultWithHits.MissCount);

        var metadataNoHits = new InMemoryCacheEntryMetadata(
            "key", now, now, null, null, true, 5, 0, 5); // 0 hits, 5 misses = 0% hit ratio
        
        var resultNoHits = CacheResult<string>.Success("value", metadataNoHits);
        Assert.Equal(0.0, resultNoHits.HitRatio);
        Assert.Equal(0, resultNoHits.HitCount);
        Assert.Equal(5, resultNoHits.MissCount);

        var resultWithoutMetadata = CacheResult<string>.NotFound("key");
        Assert.Equal(0.0, resultWithoutMetadata.HitRatio);
        Assert.Equal(0, resultWithoutMetadata.HitCount);
        Assert.Equal(0, resultWithoutMetadata.MissCount);
    }

    [Fact]
    public void CacheResult_Implicit_Conversion()
    {
        // Implicit conversion from value to successful result
        CacheResult<string> result = "test_value";
        Assert.True(result.IsSuccess);
        Assert.Equal("test_value", result.Value);

        // Implicit conversion from result to value
        CacheResult<string> successResult = CacheResult<string>.Success("success_value");
        string? value = successResult;
        Assert.Equal("success_value", value);

        // Implicit conversion from failed result returns default
        CacheResult<string> failResult = CacheResult<string>.NotFound("key");
        string? defaultValue = failResult;
        Assert.Equal(default(string), defaultValue);
    }

    [Fact]
    public void CacheResult_Pattern_Matching()
    {
        var successResult = CacheResult<string>.Success("success");
        var failureResult = CacheResult<string>.NotFound("key");

        // Deconstruction pattern matching
        var (isSuccess, value) = successResult;
        Assert.True(isSuccess);
        Assert.Equal("success", value!);

        var (isFailure, failValue) = failureResult;
        Assert.False(isFailure);
        Assert.Equal(default(string)!, failValue);

        // Switch pattern matching
        string result = successResult switch
        {
            { IsSuccess: true } => "success",
            { IsNotFound: true } => "not_found",
            { IsExpired: true } => "expired",
            _ => "error"
        };
        Assert.Equal("success", result);
    }

    [Fact]
    public void CacheEntryMetadata_Handles_Null_Parameters()
    {
        Assert.Throws<ArgumentNullException>(() => new InMemoryCacheEntryMetadata(null!, DateTime.UtcNow, DateTime.UtcNow));
        
        // These should work with null optional parameters
        var metadata = new InMemoryCacheEntryMetadata("key", DateTime.UtcNow, DateTime.UtcNow, null, null);
        Assert.Equal("key", metadata.Key);
        Assert.Null(metadata.ExpiresAtUtc);
        Assert.Null(metadata.SizeBytes);

        // Test disk metadata too
        var diskMetadata = new DiskCacheEntryMetadata("disk_key", null, DateTime.UtcNow, DateTime.UtcNow, null, null, null);
        Assert.Equal("disk_key", diskMetadata.Key);
        Assert.Null(diskMetadata.FileKind);
        Assert.Null(diskMetadata.ExpiresAtUtc);
        Assert.Null(diskMetadata.SizeBytes);
        Assert.Null(diskMetadata.FilePath);
    }
}

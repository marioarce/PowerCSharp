using PowerCSharp.Feature.Cache.Abstractions.Enums;

namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Represents the result of a cache operation, providing the value (if successful) and diagnostic information.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public sealed class CacheResult<T>
{
    /// <summary>
    /// Gets the cached value, if the operation was successful.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful (value was found).
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the cache entry was not found.
    /// </summary>
    public bool IsNotFound => !IsSuccess && Reason == CacheResultReason.NotFound;

    /// <summary>
    /// Gets a value indicating whether the cache entry was expired.
    /// </summary>
    public bool IsExpired => !IsSuccess && Reason == CacheResultReason.Expired;

    /// <summary>
    /// Gets the reason for the result status.
    /// </summary>
    public CacheResultReason Reason { get; }

    /// <summary>
    /// Gets metadata about the cache entry, if available.
    /// </summary>
    public CacheEntryMetadata? Metadata { get; }

    /// <summary>
    /// Gets the UTC timestamp when this result was generated.
    /// </summary>
    public DateTime RetrievedAtUtc { get; }

    /// <summary>
    /// Gets the name of the cache provider that generated this result.
    /// </summary>
    public string? ProviderName { get; }

    /// <summary>
    /// Gets the type of cache (Memory, Disk, etc.).
    /// </summary>
    public string? CacheType { get; }

    /// <summary>
    /// Gets the duration of the cache retrieval operation.
    /// </summary>
    public TimeSpan RetrievalDuration { get; }

    /// <summary>
    /// Gets the total time spent in cache operations for this request.
    /// </summary>
    public TimeSpan TotalCacheTime { get; }

    /// <summary>
    /// Gets the number of cache hits for this entry.
    /// </summary>
    public long HitCount => Metadata?.HitCount ?? 0;

    /// <summary>
    /// Gets the number of cache misses for this entry.
    /// </summary>
    public long MissCount => Metadata?.MissCount ?? 0;

    /// <summary>
    /// Gets the hit ratio for this entry (hits / total accesses).
    /// </summary>
    public double HitRatio
    {
        get
        {
            var total = HitCount + MissCount;
            return total > 0 ? (double)HitCount / total : 0.0;
        }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CacheResult{T}"/> for a successful operation.
    /// </summary>
    /// <param name="value">The cached value.</param>
    /// <param name="metadata">Optional metadata about the cache entry.</param>
    /// <param name="providerName">Optional name of the cache provider.</param>
    /// <param name="cacheType">Optional type of cache.</param>
    /// <param name="retrievalDuration">Optional duration of the retrieval operation.</param>
    /// <param name="totalCacheTime">Optional total time spent in cache operations.</param>
    public CacheResult(
        T value, 
        CacheEntryMetadata? metadata = null, 
        string? providerName = null, 
        string? cacheType = null,
        TimeSpan? retrievalDuration = null,
        TimeSpan? totalCacheTime = null)
    {
        Value = value;
        IsSuccess = true;
        Reason = CacheResultReason.Success;
        Metadata = metadata;
        ProviderName = providerName;
        CacheType = cacheType;
        RetrievalDuration = retrievalDuration ?? TimeSpan.Zero;
        TotalCacheTime = totalCacheTime ?? RetrievalDuration;
        RetrievedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CacheResult{T}"/> for a failed operation.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    /// <param name="metadata">Optional metadata about the cache entry.</param>
    /// <param name="providerName">Optional name of the cache provider.</param>
    /// <param name="cacheType">Optional type of cache.</param>
    /// <param name="retrievalDuration">Optional duration of the retrieval operation.</param>
    /// <param name="totalCacheTime">Optional total time spent in cache operations.</param>
    public CacheResult(
        CacheResultReason reason, 
        CacheEntryMetadata? metadata = null, 
        string? providerName = null, 
        string? cacheType = null,
        TimeSpan? retrievalDuration = null,
        TimeSpan? totalCacheTime = null)
    {
        Value = default;
        IsSuccess = false;
        Reason = reason;
        Metadata = metadata;
        ProviderName = providerName;
        CacheType = cacheType;
        RetrievalDuration = retrievalDuration ?? TimeSpan.Zero;
        TotalCacheTime = totalCacheTime ?? RetrievalDuration;
        RetrievedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful cache result.
    /// </summary>
    /// <param name="value">The cached value.</param>
    /// <param name="metadata">Optional metadata about the cache entry.</param>
    /// <param name="providerName">Optional name of the cache provider.</param>
    /// <param name="cacheType">Optional type of cache.</param>
    /// <param name="retrievalDuration">Optional duration of the retrieval operation.</param>
    /// <returns>A successful cache result.</returns>
    public static CacheResult<T> Success(
        T value, 
        CacheEntryMetadata? metadata = null, 
        string? providerName = null, 
        string? cacheType = null,
        TimeSpan? retrievalDuration = null)
        => new CacheResult<T>(value, metadata, providerName, cacheType, retrievalDuration);

    /// <summary>
    /// Creates a cache result indicating the entry was not found.
    /// </summary>
    /// <param name="key">The key that was not found.</param>
    /// <param name="providerName">Optional name of the cache provider.</param>
    /// <param name="cacheType">Optional type of cache.</param>
    /// <param name="retrievalDuration">Optional duration of the retrieval operation.</param>
    /// <returns>A not-found cache result.</returns>
    public static CacheResult<T> NotFound(
        string key, 
        string? providerName = null, 
        string? cacheType = null,
        TimeSpan? retrievalDuration = null)
        => new CacheResult<T>(CacheResultReason.NotFound, null, providerName, cacheType, retrievalDuration);

    /// <summary>
    /// Creates a cache result indicating the entry was expired.
    /// </summary>
    /// <param name="metadata">Metadata about the expired entry.</param>
    /// <param name="providerName">Optional name of the cache provider.</param>
    /// <param name="cacheType">Optional type of cache.</param>
    /// <param name="retrievalDuration">Optional duration of the retrieval operation.</param>
    /// <returns>An expired cache result.</returns>
    public static CacheResult<T> Expired(
        CacheEntryMetadata metadata, 
        string? providerName = null, 
        string? cacheType = null,
        TimeSpan? retrievalDuration = null)
        => new CacheResult<T>(CacheResultReason.Expired, metadata, providerName, cacheType, retrievalDuration);

    /// <summary>
    /// Creates a cache result indicating an error occurred.
    /// </summary>
    /// <param name="key">The key that caused the error.</param>
    /// <param name="providerName">Optional name of the cache provider.</param>
    /// <param name="cacheType">Optional type of cache.</param>
    /// <param name="retrievalDuration">Optional duration of the retrieval operation.</param>
    /// <returns>An error cache result.</returns>
    public static CacheResult<T> Error(
        string key, 
        string? providerName = null, 
        string? cacheType = null,
        TimeSpan? retrievalDuration = null)
        => new CacheResult<T>(CacheResultReason.Error, null, providerName, cacheType, retrievalDuration);

    /// <summary>
    /// Gets the value if the operation was successful, otherwise returns the default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if unsuccessful.</param>
    /// <returns>The cached value or default.</returns>
    public T? GetValueOrDefault(T? defaultValue = default)
        => IsSuccess ? Value : defaultValue;

    /// <summary>
    /// Gets the value if the operation was successful, otherwise throws an exception.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the operation was not successful.</exception>
    /// <returns>The cached value.</returns>
    public T GetValueOrThrow()
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException($"Cache operation failed: {Reason}");
        }

        return Value!;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsSuccess)
        {
            var sizeInfo = Metadata?.SizeBytes.HasValue == true ? $" ({Metadata.SizeBytes.Value} bytes)" : "";
            var providerInfo = !string.IsNullOrEmpty(ProviderName) ? $" [{ProviderName}]" : "";
            var typeInfo = !string.IsNullOrEmpty(CacheType) ? $" ({CacheType})" : "";
            var timingInfo = RetrievalDuration > TimeSpan.Zero ? $" in {RetrievalDuration.TotalMilliseconds:F2}ms" : "";
            var hitRatioInfo = HitRatio > 0 ? $" (hit ratio: {HitRatio:P1})" : "";
            
            return $"CacheResult.Success{providerInfo}{typeInfo}: {typeof(T).Name}{sizeInfo}{timingInfo}{hitRatioInfo}";
        }
        else
        {
            var providerInfo = !string.IsNullOrEmpty(ProviderName) ? $" [{ProviderName}]" : "";
            var typeInfo = !string.IsNullOrEmpty(CacheType) ? $" ({CacheType})" : "";
            var timingInfo = RetrievalDuration > TimeSpan.Zero ? $" in {RetrievalDuration.TotalMilliseconds:F2}ms" : "";
            
            return $"CacheResult.{Reason}{providerInfo}{typeInfo}: {typeof(T).Name}{timingInfo}";
        }
    }

    /// <summary>
    /// Implicit conversion to the cached value.
    /// </summary>
    /// <param name="result">The cache result.</param>
    public static implicit operator T?(CacheResult<T> result) => result.Value;

    /// <summary>
    /// Implicit conversion from a value to a successful cache result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CacheResult<T>(T value) => Success(value);

    /// <summary>
    /// Pattern matching support for deconstruction.
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful.</param>
    /// <param name="value">The cached value (if successful).</param>
    public void Deconstruct(out bool isSuccess, out T? value)
    {
        isSuccess = IsSuccess;
        value = Value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CacheResult<T> other && Equals(other);

    /// <inheritdoc />
    public bool Equals(CacheResult<T>? other)
    {
        if (other == null)
        {
            return false;
        }
        
        return IsSuccess == other.IsSuccess &&
               Reason == other.Reason &&
               EqualityComparer<T?>.Default.Equals(Value, other.Value) &&
               EqualityComparer<CacheEntryMetadata?>.Default.Equals(Metadata, other.Metadata) &&
               string.Equals(ProviderName, other.ProviderName, StringComparison.Ordinal) &&
               string.Equals(CacheType, other.CacheType, StringComparison.Ordinal) &&
               RetrievalDuration.Equals(other.RetrievalDuration) &&
               TotalCacheTime.Equals(other.TotalCacheTime);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 31 + IsSuccess.GetHashCode();
        hash = hash * 31 + Reason.GetHashCode();
        hash = hash * 31 + (Value?.GetHashCode() ?? 0);
        hash = hash * 31 + (Metadata?.GetHashCode() ?? 0);
        hash = hash * 31 + (ProviderName?.GetHashCode() ?? 0);
        hash = hash * 31 + (CacheType?.GetHashCode() ?? 0);
        hash = hash * 31 + RetrievalDuration.GetHashCode();
        hash = hash * 31 + TotalCacheTime.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(CacheResult<T>? left, CacheResult<T>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(CacheResult<T>? left, CacheResult<T>? right) => !(left == right);
}

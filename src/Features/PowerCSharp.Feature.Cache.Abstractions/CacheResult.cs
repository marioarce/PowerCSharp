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
    /// Initializes a new instance of <see cref="CacheResult{T}"/> for a successful operation.
    /// </summary>
    /// <param name="value">The cached value.</param>
    /// <param name="metadata">Optional metadata about the cache entry.</param>
    public CacheResult(T value, CacheEntryMetadata? metadata = null)
    {
        Value = value;
        IsSuccess = true;
        Reason = CacheResultReason.Success;
        Metadata = metadata;
        RetrievedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CacheResult{T}"/> for a failed operation.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    /// <param name="metadata">Optional metadata about the cache entry.</param>
    public CacheResult(CacheResultReason reason, CacheEntryMetadata? metadata = null)
    {
        Value = default;
        IsSuccess = false;
        Reason = reason;
        Metadata = metadata;
        RetrievedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful cache result.
    /// </summary>
    /// <param name="value">The cached value.</param>
    /// <param name="metadata">Optional metadata about the cache entry.</param>
    /// <returns>A successful cache result.</returns>
    public static CacheResult<T> Success(T value, CacheEntryMetadata? metadata = null)
        => new CacheResult<T>(value, metadata);

    /// <summary>
    /// Creates a cache result indicating the entry was not found.
    /// </summary>
    /// <param name="key">The key that was not found.</param>
    /// <returns>A not-found cache result.</returns>
    public static CacheResult<T> NotFound(string key)
        => new CacheResult<T>(CacheResultReason.NotFound, null);

    /// <summary>
    /// Creates a cache result indicating the entry was expired.
    /// </summary>
    /// <param name="metadata">Metadata about the expired entry.</param>
    /// <returns>An expired cache result.</returns>
    public static CacheResult<T> Expired(CacheEntryMetadata metadata)
        => new CacheResult<T>(CacheResultReason.Expired, metadata);

    /// <summary>
    /// Creates a cache result indicating an error occurred.
    /// </summary>
    /// <param name="key">The key that caused the error.</param>
    /// <returns>An error cache result.</returns>
    public static CacheResult<T> Error(string key)
        => new CacheResult<T>(CacheResultReason.Error, null);

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
            return $"CacheResult.Success: {typeof(T).Name}{sizeInfo}";
        }
        else
        {
            return $"CacheResult.{Reason}: {typeof(T).Name}";
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CacheResult<T> other && Equals(other);

    /// <inheritdoc />
    public bool Equals(CacheResult<T>? other)
    {
        if (other == null) return false;
        
        return IsSuccess == other.IsSuccess &&
               Reason == other.Reason &&
               EqualityComparer<T?>.Default.Equals(Value, other.Value) &&
               EqualityComparer<CacheEntryMetadata?>.Default.Equals(Metadata, other.Metadata);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 31 + IsSuccess.GetHashCode();
        hash = hash * 31 + Reason.GetHashCode();
        hash = hash * 31 + (Value?.GetHashCode() ?? 0);
        hash = hash * 31 + (Metadata?.GetHashCode() ?? 0);

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

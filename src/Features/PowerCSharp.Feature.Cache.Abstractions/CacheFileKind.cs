namespace PowerCSharp.Feature.Cache.Abstractions;

/// <summary>
/// Smart-enum representing different kinds of cache files for categorization and management.
/// Provides a static registry for discovering and working with cache file types.
/// </summary>
public sealed class CacheFileKind : IEquatable<CacheFileKind>
{
    /// <summary>
    /// Gets the unique identifier for this cache file kind.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name for this cache file kind.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the file extension typically associated with this cache file kind.
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// Gets a description of this cache file kind.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets all registered cache file kinds.
    /// </summary>
    public static IReadOnlyList<CacheFileKind> All
    {
        get
        {
            lock (_registryLock)
            {
                return _registry.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets a cache file kind by its identifier.
    /// </summary>
    public static CacheFileKind? GetById(string id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        lock (_registryLock)
        {
            return _registry.FirstOrDefault(k => k.Id == id);
        }
    }

    /// <summary>
    /// Registers a new cache file kind.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if a kind with the same ID already exists.</exception>
    public static CacheFileKind Register(string id, string name, string extension, string description)
    {
        var kind = new CacheFileKind(id, name, extension, description);

        lock (_registryLock)
        {
            if (_registry.Any(k => k.Id == id))
            {
                throw new ArgumentException($"CacheFileKind with ID '{id}' is already registered.", nameof(id));
            }

            _registry.Add(kind);
        }

        return kind;
    }

    /// <summary>
    /// Determines if this cache file kind matches the given file path.
    /// </summary>
    public bool MatchesFile(string filePath)
    {
        if (filePath == null)
        {
            return false;
        }
        return Path.GetExtension(filePath).Equals(Extension, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if this cache file kind matches the given file extension.
    /// </summary>
    public bool MatchesExtension(string extension)
    {
        if (extension == null)
        {
            return false;
        }
        return extension.Equals(Extension, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CacheFileKind other && Equals(other);

    /// <inheritdoc />
    public bool Equals(CacheFileKind? other) => other != null && Id == other.Id;

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(CacheFileKind? left, CacheFileKind? right)
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
    public static bool operator !=(CacheFileKind? left, CacheFileKind? right) => !(left == right);

    // Private fields and constructor (moved to end)
    private static readonly List<CacheFileKind> _registry = new();
    private static readonly object _registryLock = new();

    private CacheFileKind(string id, string name, string extension, string description)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Extension = extension ?? throw new ArgumentNullException(nameof(extension));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    // Built-in cache file kinds
    static CacheFileKind()
    {
        // Register standard cache file kinds
        Register("json", "JSON Data", ".json", "Serialized JSON cache data files");
        Register("binary", "Binary Data", ".bin", "Binary serialized cache data files");
        Register("compressed", "Compressed Data", ".gz", "Compressed cache data files");
        Register("metadata", "Metadata", ".meta", "Cache metadata and index files");
        Register("temp", "Temporary", ".tmp", "Temporary cache files during writes");
    }
}

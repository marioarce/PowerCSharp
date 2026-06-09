namespace PowerCSharp.Features.Registry;

/// <summary>
/// Captures the resolved feature matrix (discovered / enabled / source) for diagnostics.
/// Registered as a singleton by <c>AddPowerFeatures</c>.
/// </summary>
public sealed class FeatureRegistry
{
    private readonly List<FeatureRegistryEntry> _entries = new();

    /// <summary>All recorded feature entries, in discovery order.</summary>
    public IReadOnlyList<FeatureRegistryEntry> Entries => _entries;

    /// <summary>Records a feature entry.</summary>
    public void Add(FeatureRegistryEntry entry) => _entries.Add(entry);
}

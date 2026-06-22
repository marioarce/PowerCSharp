using System.Reflection;
using PowerCSharp.Features.Abstractions;

namespace PowerCSharp.Features.Internal;

/// <summary>
/// Discovers <see cref="IFeatureModule"/> implementations from opted-in assemblies and
/// merges them with explicitly supplied module instances. De-duplicates by concrete type.
/// </summary>
internal static class FeatureModuleDiscovery
{
    public static IReadOnlyList<IFeatureModule> Discover(
        IEnumerable<Assembly> assemblies,
        IEnumerable<IFeatureModule> explicitModules)
    {
        var byType = new Dictionary<Type, IFeatureModule>();

        foreach (var module in explicitModules)
        {
            byType[module.GetType()] = module;
        }

        foreach (var assembly in assemblies.Distinct())
        {
            var types = GetLoadableTypes(assembly);

            foreach (var type in types)
            {
                if (!typeof(IFeatureModule).IsAssignableFrom(type))
                {
                    continue;
                }

                if (type.IsAbstract || type.IsInterface || byType.ContainsKey(type))
                {
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) is null)
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is IFeatureModule module)
                {
                    byType[type] = module;
                }
            }
        }

        return byType.Values
            .OrderBy(m => m.Order)
            .ThenBy(m => m.FeatureKey, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}

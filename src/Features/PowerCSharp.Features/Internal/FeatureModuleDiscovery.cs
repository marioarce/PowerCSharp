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

        Console.WriteLine($"[Discovery] Processing {explicitModules.Count()} explicit modules");
        foreach (var module in explicitModules)
        {
            byType[module.GetType()] = module;
            Console.WriteLine($"[Discovery] Added explicit module: {module.FeatureKey} (Type: {module.GetType().Name})");
        }

        foreach (var assembly in assemblies.Distinct())
        {
            Console.WriteLine($"[Discovery] Processing assembly: {assembly.FullName}");
            var types = GetLoadableTypes(assembly).ToList();
            Console.WriteLine($"[Discovery] Found {types.Count} types in assembly");
            
            var moduleTypes = new List<Type>();
            foreach (var type in types)
            {
                if (!typeof(IFeatureModule).IsAssignableFrom(type))
                {
                    continue;
                }

                if (type.IsAbstract || type.IsInterface || byType.ContainsKey(type))
                {
                    Console.WriteLine($"[Discovery] Skipping IFeatureModule type '{type.Name}' (abstract/interface/duplicate)");
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) is null)
                {
                    Console.WriteLine($"[Discovery] Skipping IFeatureModule type '{type.Name}' (no parameterless constructor)");
                    continue;
                }

                moduleTypes.Add(type);
                if (Activator.CreateInstance(type) is IFeatureModule module)
                {
                    byType[type] = module;
                    Console.WriteLine($"[Discovery] Successfully created module: {module.FeatureKey} (Type: {type.Name})");
                }
                else
                {
                    Console.WriteLine($"[Discovery] Failed to create instance of IFeatureModule type: {type.Name}");
                }
            }
            
            Console.WriteLine($"[Discovery] Found {moduleTypes.Count} IFeatureModule types in {assembly.FullName}");
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

using System.Text;
using Common.Core;
using MoreLinq;

namespace Common.Registry;

public interface IRegistry<T> : IService where T : IRegisterable
{
    IDictionary<uint, T> ObjectMapping { get; }
    IDictionary<string, uint> IdMapping { get; }
}

public static class RegistryExtensions
{
    private static readonly Dictionary<Type, uint> IdCounter = new();
    
    public static T Resolve<T>(this IRegistry<T> registry, string id) where T : class, IRegisterable
    {
        return registry.ObjectMapping[registry.IdMapping[id]];
    }

    public static bool Register<T>(this IRegistry<T> registry, T registerable, bool overrideIfExists = false) where T : class, IRegisterable
    {
        var type = registerable.GetType();

        // Mapping has been loaded or an object is already registered.
        // Note: You CANNOT assume that an object is already registered even if the mapping exists.
        if (registry.IdMapping.TryGetValue(registerable.Id, out var value))
        {
            registerable.MappedId = value;
        }
        else
        {
            // TryAdd should only return true if no mappings are loaded from save data, such as a new registry after an update or a new game.
            IdCounter.TryAdd(type, 0);
            IdCounter[type] = ++IdCounter[type];
            
            registerable.MappedId = IdCounter[type];
            
            registry.IdMapping[registerable.Id] = registerable.MappedId;
        }
        
        if (!overrideIfExists && registry.ObjectMapping.ContainsKey(registerable.MappedId))
            return false;
        
        registry.ObjectMapping[registerable.MappedId] = registerable;
        return true;
    }

    public static byte[] SerializeMappings<T>(this IRegistry<T> registry) where T : class, IRegisterable
    {
        using var stream = new MemoryStream();
        
        var count = registry.IdMapping.Count;
        
        stream.Write(BitConverter.GetBytes(count), 0, sizeof(int));
        
        foreach (var mapping in registry.IdMapping)
        {
            stream.Write(BitConverter.GetBytes(mapping.Value), 0, sizeof(uint));
            
            // Max of 32 chars is probably overkill.
            stream.Write(Encoding.UTF8.GetBytes(mapping.Key), 0, 32);
        }
        
        return stream.ToArray();
    }
    
    public static void LoadSerializedMappings<T>(this IRegistry<T> registry, byte[] serializedMappings) where T : class, IRegisterable
    {
        throw new NotImplementedException();
    }
    
    // Fixes id mappings with no corresponding object after registering is complete.
    // This will allow us to assume an object exists if its id mapping exists after this function is called.
    public static void FixMappings<T>(this IRegistry<T> registry) where T : class, IRegisterable
    {
        throw new NotImplementedException();
    }
}
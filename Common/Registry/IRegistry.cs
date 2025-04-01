using Common.Core;

namespace Common.Registry;

public interface IRegistry<T> : IService where T : IRegisterable
{
    T Resolve(string id);
    T Resolve(uint id);
    
    // Passes in an unregistered IRegisterable and returns it with its 'MappedId' set.
    T Register(T registerable, bool overrideIfExists = false);
}
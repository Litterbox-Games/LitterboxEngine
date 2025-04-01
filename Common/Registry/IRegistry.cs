using Common.Core;

namespace Common.Registry;

public interface IRegistry<T> : IService where T : IRegisterable
{
    IRegisterable Resolve(string id);
    IRegisterable Resolve(uint id);
    
    // Passes in an unregistered IRegisterable and returns it with its 'MappedId' set.
    IRegisterable Register(IRegisterable registerable, bool overrideIfExists = false);
}
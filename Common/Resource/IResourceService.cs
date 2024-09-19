using Common.DI;

namespace Common.Resource;

public interface IResourceService : IService
{
    public T Get<T>(string path) where T : IResource;
}
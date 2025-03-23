using Common.Core;

namespace Common.Services.Resource;

public interface IResourceService : IService
{
    public T Get<T>(string path) where T : IResource;
}
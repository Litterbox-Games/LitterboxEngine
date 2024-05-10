using Common.Resource;

namespace Client.Graphics;

public interface IResourceService
{
    public T Get<T>(string path) where T : IResource;
}
namespace Common.Resource;

public interface IResourceService
{
    public T Get<T>(string path) where T : IResource;
}
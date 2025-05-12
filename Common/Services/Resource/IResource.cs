namespace Common.Services.Resource;

public interface IResource
{
    public IResource LoadFromFile(string path);
}

/// <summary>
///     A contract representing a loadable resource.
/// </summary>
public interface IResource<out T>: IResource where T : IResource, IResource<T>
{
    /// <summary>
    ///     Loads a resource from a file.
    /// </summary>
    /// <param name="path">The path used to locate the resource.</param>
    /// <returns>An instance of the resource.</returns>
    public new static abstract T LoadFromFile(string path);
    
    IResource IResource.LoadFromFile(string path) => T.LoadFromFile(path);
}

public interface IReloadable;
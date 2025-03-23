namespace Common.Resource;

/// <summary>
///     A contract representing a loadable resource.
/// </summary>
public interface IResource
{
    /// <summary>
    ///     Loads a resource from a file.
    /// </summary>
    /// <param name="path">The path used to locate the resource.</param>
    /// <returns>An instance of the resource.</returns>
    public static virtual IResource LoadFromFile(string path)
    {
        throw new NotImplementedException();
    }
}

public interface IReloadable
{
    public IResource Reload(string path);
}
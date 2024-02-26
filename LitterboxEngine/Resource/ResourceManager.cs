using LitterboxEngine.Resource.Exceptions;
using MoreLinq;

namespace LitterboxEngine.Resource;

/// <summary>
///     A static resource handler to load/store/dispose of resources.
/// </summary>
public static class ResourceManager
{
    private static readonly Dictionary<string, IResource> Resources = new ();

    // TODO: Add this back when logger is moved over
    /*
    private static ILoggingService? _logger;
    
    /// <summary>
    ///     Passes the resource loader a logging service. This is not resolved manually as the ResourceManager is not a service.
    /// </summary>
    /// <param name="logger"></param>
    public static void SetLogger(ILoggingService logger)
    {
        _logger = logger;
    }
    */
    
    /// <summary>
    ///     Get a resource of a specific type at a given path.
    /// </summary>
    /// <param name="path">The path to locate the resource.</param>
    /// <typeparam name="T">The type of resource expected.</typeparam>
    /// <returns>An instance of the loaded resource.</returns>
    /// <exception cref="ResourceFileNotFoundException">The resource file at the given path was not found.</exception>
    /// <exception cref="ResourceLoadingFailedException">The resource file was failed, but failed to load.</exception>
    public static T Get<T>(string path) where T : IResource
    {
        path = path.StartsWith("Resources/") ? path : $"Resources/{path}";

        if (!File.Exists(path))
            throw new ResourceFileNotFoundException($"Attempting to load resource at \"{path}\" has failed, this file does not exist.");
        
        // _logger?.Information($"Loading resource '{path}'...");
        
        if (Resources.TryGetValue(path, out var value))
            return (T)value;
        
        var r = (T)T.LoadFromFile(path);
        Resources[path] = r;
        return r;
    }

    /// <summary>
    ///     Dispose of any disposable resources.
    /// </summary>
    public static void DisposeResources()
    {
        var disposables = Resources.Select(x => x.Value).Where(x => x.GetType().IsAssignableTo(typeof(IDisposable))).Cast<IDisposable>();
        
        disposables.ForEach(x => x.Dispose());
        
        Resources.Clear();
    }
}
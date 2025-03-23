using Common.Logging;
using Common.Resource.Exceptions;
using MoreLinq;

namespace Common.Resource;

public class ServerResourceService: IResourceService, IDisposable
{
    private readonly Dictionary<string, IResource> _resources = new ();
    private readonly ILoggingService _logger;

    public ServerResourceService(ILoggingService logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Get a resource of a specific type at a given path.
    /// </summary>
    /// <param name="path">The path to locate the resource.</param>
    /// <typeparam name="T">The type of resource expected.</typeparam>
    /// <returns>An instance of the loaded resource.</returns>
    /// <exception cref="ResourceFileNotFoundException">The resource file at the given path was not found.</exception>
    /// <exception cref="ResourceLoadingFailedException">The resource file was failed, but failed to load.</exception>
    public T Get<T>(string path) where T : IResource
    {
        path = path.StartsWith("Resources/") ? path : $"Resources/{path}";

        if (!File.Exists(path))
            throw new ResourceFileNotFoundException($"Attempting to load resource at \"{path}\" has failed, this file does not exist.");
        
        _logger.Information($"Loading resource '{path}'...");
        
        if (_resources.TryGetValue(path, out var value))
            return (T)value;
        
        var r = (T)T.LoadFromFile(path);
        _resources[path] = r;
        return r;
    }

    /// <summary>
    ///     Dispose of any disposable resources.
    /// </summary>
    public void Dispose()
    {
        var disposables = _resources.Select(x => x.Value).Where(x => x.GetType().IsAssignableTo(typeof(IDisposable))).Cast<IDisposable>();
        
        disposables.ForEach(x => x.Dispose());
        
        _resources.Clear();
        GC.SuppressFinalize(this);
    }
}
using Client.Graphics.GHAL;
using Common.Logging;
using Common.Resource;
using Common.Resource.Exceptions;
using MoreLinq;

namespace Client.Resource;

public class ClientResourceService: IResourceService, IDisposable
{
    private readonly Dictionary<string, IResource> _resources = new ();
    private readonly ILoggingService _logger;
    private readonly IGraphicsDeviceService _graphicsDeviceService;

    public ClientResourceService(ILoggingService logger, IGraphicsDeviceService graphicsDeviceService)
    {
        _logger = logger;
        _graphicsDeviceService = graphicsDeviceService;
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

        var resource = (T)T.LoadFromFile(path);

        if (resource is IGraphicsResource graphicsResource)
            resource = (T)graphicsResource.UploadToGraphicsDevice(_graphicsDeviceService);

        _resources[path] = resource;
        return resource;
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
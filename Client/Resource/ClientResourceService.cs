using Client.Graphics.GHAL;
using Common.DI;
using Common.Logging;
using Common.Resource;
using Common.Resource.Exceptions;
using MoreLinq;

namespace Client.Resource;

public class ClientResourceService: IResourceService, ITickableService, IDisposable
{
    private readonly Dictionary<string, IResource> _resources = new ();
    private readonly ILoggingService _logger;
    private readonly IGraphicsDeviceService _graphicsDeviceService;
    private readonly FileSystemWatcher _watcher;

    private readonly HashSet<string> _resourcesToReload = new();

    public ClientResourceService(ILoggingService logger, IGraphicsDeviceService graphicsDeviceService)
    {
        _logger = logger;
        _graphicsDeviceService = graphicsDeviceService;

        _watcher = new FileSystemWatcher
        {
            Path = "../../../Resources",
            NotifyFilter = NotifyFilters.LastWrite,
            Filters =  { "*.aseprite" },
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
    }
    
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed) return;

        var info = new FileInfo(e.FullPath);
        // Hack: Aseprite will write to files in batches of 2, ignore the first write
        if (info is {Extension: ".aseprite", Length: < 50}) return;

        _resourcesToReload.Add(e.FullPath);
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
        
        if (_resources.TryGetValue(path, out var value))
            return (T)value;
        
        _logger.Information($"Loading resource '{path}'...");

        var resource = (T)T.LoadFromFile(path);

        if (resource is IGraphicsResource graphicsResource)
            resource = (T)graphicsResource.UploadToGraphicsDevice(_graphicsDeviceService);

        _resources[path] = resource;
        return resource;
    }

    public void Update(float deltaTime)
    {
        foreach (var resourceToReload in _resourcesToReload)
        {
            var path = resourceToReload["../../../".Length..].Replace('\\', '/');
        
            // Don't reload a resource we aren't even using
            if (!_resources.TryGetValue(path, out var oldResource)) return;

            // Don't reload a resource that isn't reloadable
            if (oldResource is not IReloadable reloadableResource) return;
            
            _logger.Information($"Reloading resource '{path}'...");
            
            File.Copy(resourceToReload, path, true);

            var resource = reloadableResource.Reload(path);
            
            if (oldResource is IDisposable disposableResource) {
                _graphicsDeviceService.WaitIdle();
                disposableResource.Dispose();
            }
            
            if (resource is IGraphicsResource graphicsResource) {
                _graphicsDeviceService.WaitIdle();
                resource = graphicsResource.UploadToGraphicsDevice(_graphicsDeviceService);
            }
            
            _resources[path] = resource;
        }
        
        _resourcesToReload.Clear();
    }
    
    /// <summary>
    ///     Dispose of any disposable resources.
    /// </summary>
    public void Dispose()
    {
        _watcher.Dispose();
        
        var disposables = _resources.Select(x => x.Value).Where(x => x.GetType().IsAssignableTo(typeof(IDisposable))).Cast<IDisposable>();
        
        disposables.ForEach(x => x.Dispose());
        
        _resources.Clear();
        GC.SuppressFinalize(this);
    }

    public void Draw() { }
}
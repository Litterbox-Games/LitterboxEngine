using LitterboxEngine.Graphics;
using LitterboxEngine.Graphics.GHAL;
using LitterboxEngine.Resource;

namespace LitterboxEngine;

public class Engine: IDisposable
{
    private readonly IGame _game;
    private readonly Window _window;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Renderer _renderer;
    private bool _isRunning;

    public Engine(string title, IGame game)
    {
        _game = game;
        _window = new Window(title);
        _graphicsDevice = GraphicsDevice.Create(_window, new GraphicsDeviceDescription(), GraphicsBackend.Vulkan);
        ResourceManager.SetGraphicsDevice(_graphicsDevice);
        _renderer = new Renderer(_window, _graphicsDevice);
        _game.Init(_window);
    }

    private void Run()
    {
        while (_isRunning && !_window.ShouldClose())
        {
            _window.PollEvents();
            _game.Input(_window);
            _game.Update(_window);
            _game.Draw(_renderer);
        }
        
        _graphicsDevice.WaitIdle();
    }

    public void Start()
    {
        _isRunning = true;
        Run();
    }

    public void Stop()
    {
        _isRunning = false;
    }
    
    public void Dispose()
    {
        _game.Dispose();
        _renderer.Dispose();
        _graphicsDevice.Dispose();
        _window.Dispose();
        GC.SuppressFinalize(this);
    }
}
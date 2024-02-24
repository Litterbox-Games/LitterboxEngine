using LitterboxEngine.Graphics;

namespace LitterboxEngine;

public class Engine: IDisposable
{
    private readonly IGame _game;
    private readonly Window _window;
    private readonly VulkanRenderer _vulkanRenderer;
    private bool _isRunning;

    public Engine(string title, IGame game)
    {
        _game = game;
        _window = new Window(title);
        _vulkanRenderer = new VulkanRenderer(_window);
        _game.Init(_window);
    }

    private void Run()
    {
        while (_isRunning && !_window.ShouldClose())
        {
            _window.PollEvents();
            _game.Input(_window);
            _game.Update(_window);
            _vulkanRenderer.Render();
        }
        
        _vulkanRenderer.DeviceWaitIdle();
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
        _vulkanRenderer.Dispose();
        _window.Dispose();
        GC.SuppressFinalize(this);
    }
}
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Autofac;
using Autofac.Features.AttributeFilters;
using Client.Core.Extensions;
using Client.Graphics;
using Client.Graphics.Backend;
using Client.Graphics.Backend.Vulkan;
using Client.Graphics.ImGui;
using Client.Host;
using Common.Core;
using Common.Core.Extensions;
using Common.Services.Logging;
using Silk.NET.Input;

namespace Client.Services.UI;

[Engine]
public class GameLoopService: IService
{
    private readonly WindowService _window;
    private readonly IGraphicsDeviceService _graphicsDevice;
    private readonly RendererService _renderer;
    private readonly RendererService _guiRenderer;
    private readonly InputService _input;
    private readonly ImGuiRendererService _imGui;
    private readonly ILifetimeScope _engineScope;
    
    private IClientHost? _host;
    
    public GameLoopService
    (
        WindowService window, 
        VulkanGraphicsDeviceService graphicsDevice, 
        [KeyFilter("Game")] RendererService renderer,
        [KeyFilter("Gui")] RendererService guiRenderer,
        ImGuiRendererService imGui, 
        InputService input, 
        RootLoggingService logger,
        ILifetimeScope engineScope
    )
    {
        _window = window;
        _graphicsDevice = graphicsDevice;
        _renderer = renderer;
        _guiRenderer = guiRenderer;
        _engineScope = engineScope;
        _input = input;
        _imGui = imGui;

        logger.RefreshLoggers(engineScope);
    }

    public void Run()
    {
        var stopWatch = new Stopwatch();
        float deltaTime = 0;
        
        var engineUpdatables = _engineScope.RegisterUpdatables();
        var engineDrawables = _engineScope.RegisterDrawables();
        var engineGuiDrawables = _engineScope.RegisterGuiDrawables();
        
        while (!_window.IsClosing())
        {
            stopWatch.Start();
            _window.PollEvents();

            // TODO: is there a way we can get rid of these checks?
            if (_host == null)
            {
                engineUpdatables.ForEach(x => x.Item2.Update(deltaTime));
            }
            
            _host?.Input(_input);
            _host?.Update(deltaTime);
            
            // Must be called at the same rate as _imGui.Draw()
            _imGui.Update(deltaTime);

            _graphicsDevice.BeginFrame(Color.Black);
            
                _renderer.BeginDrawing();
                
                    if (_host == null)
                    {
                        engineDrawables.ForEach(drawable => drawable.Draw(deltaTime, _renderer));
                    }
                 
                    _host?.Draw(deltaTime, _renderer);
                    
                _renderer.EndDrawing();
                
                _guiRenderer.ViewMatrix = 
                    Matrix4x4.CreateOrthographicOffCenter(0f, _window.Width, 0f, _window.Height, -1f, 1f);
                
                _guiRenderer.BeginDrawing();
                
                if (_host == null)
                {
                    engineGuiDrawables.ForEach(drawable => drawable.DrawGui(deltaTime, _guiRenderer));
                }
                 
                _host?.DrawGui(deltaTime, _guiRenderer);
                    
                _guiRenderer.EndDrawing();
            
                _imGui.Draw();
            
            _graphicsDevice.EndFrame();

            if (_input.IsKeyDown(Key.Escape))
                _window.SetShouldClose();

            if (_input.IsKeyDown(Key.X))
            {
                StopGame();
            }
            
            stopWatch.Stop();
            deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
            stopWatch.Reset();
        }
    }

    public void StartGame(IClientHost host)
    {
        _host = host;
        _host.Start(_engineScope);
    }

    public bool IsGameRunning() => _host is not null;
    
    public void StopGame()
    {
        if (!IsGameRunning()) return;
        
        _graphicsDevice.WaitIdle();
                
        _host!.Stop();
        _host.Dispose();
        _host = null;
                
        _engineScope.Resolve<RootLoggingService>().RefreshLoggers(_engineScope);
                
        GC.Collect();
    }
    
    public void Dispose()
    {
        _imGui.Dispose();
        GC.SuppressFinalize(this);
    }
}
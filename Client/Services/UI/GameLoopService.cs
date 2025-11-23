using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Autofac;
using Autofac.Features.AttributeFilters;
using Client.Core.Extensions;
using Client.Graphics;
using Client.Graphics.Backend;
using Client.Graphics.ImGui;
using Client.Services.Network;
using Common.Core;
using Common.Core.Extensions;
using Common.Services.Entities;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Players;
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
    private readonly RootLoggingService _logger;
    
    private ILifetimeScope? _gameScope;
    
    private List<(float, IUpdatable)> _updatables = [];
    private List<IDrawable> _drawables = [];
    private List<IGuiDrawable> _guiDrawables = [];
    private List<IInputable> _inputables = [];
    
    
    public GameLoopService
    (
        WindowService window, 
        IGraphicsDeviceService graphicsDevice, 
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
        _logger = logger;

        _logger.RefreshLoggers(engineScope);
    }

    public void Run()
    {
        var stopWatch = new Stopwatch();
        float deltaTime = 0;
        
        _updatables = _engineScope.RegisterUpdatables();
        _inputables = _engineScope.RegisterInputables();
        _drawables = _engineScope.RegisterDrawables();
        _guiDrawables = _engineScope.RegisterGuiDrawables();
        
        while (!_window.IsClosing())
        {
            stopWatch.Start();
            
            _inputables.ForEach(inputable => inputable.Input(_input));
            _updatables.ForEach(updatable => updatable.Item2.Update(deltaTime));

            _graphicsDevice.BeginFrame(Color.Black);
            
                _renderer.BeginDrawing();
                _drawables.ForEach(drawable => drawable.Draw(deltaTime, _renderer));
                _renderer.EndDrawing();
                
                // TODO: this logic should probably live elsewhere, and be triggered by window size change event
                _guiRenderer.ViewMatrix = Matrix4x4.CreateOrthographicOffCenter(0f, _window.Width, 0f, _window.Height, -1f, 1f);
                
                _guiRenderer.BeginDrawing();
                _guiDrawables.ForEach(drawable => drawable.DrawGui(deltaTime, _guiRenderer));
                _guiRenderer.EndDrawing();
            
                _imGui.Draw();
            
            _graphicsDevice.EndFrame();

            if (_input.IsKeyDown(Key.Escape))
                _window.SetShouldClose();

            if (_input.IsKeyDown(Key.X))
                StopGame();
            
            
            stopWatch.Stop();
            deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
            stopWatch.Reset();
        }
    }

    public bool IsGameRunning() => _gameScope is not null;
    
    public void StartGame(EMode mode, bool isMultiplayer)
    {
        _gameScope = _engineScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterGameServices(mode, isMultiplayer);
        });
        
        _updatables = _gameScope.RegisterUpdatables();
        _inputables = _gameScope.RegisterInputables();
        _drawables = _gameScope.RegisterDrawables();
        _guiDrawables = _gameScope.RegisterGuiDrawables();
        
        _logger.RefreshLoggers(_gameScope);

        // TODO: these should be triggered via events rather than directly here
        if (mode == EMode.Host)
        {
            if (isMultiplayer)
            {
                var networking = _gameScope.Resolve<ServerNetworkService>();
                networking.Listen(7777);
            }

            var mobController = _gameScope.Resolve<MobControllerService>();
            for (var x = 0; x < 30; x++)
            {
                for (var y = 0; y < 30; y++)
                {
                    mobController.SpawnMobEntity(new Vector2(x * 2, y * 2));
                }
            }

            var playerService = _gameScope.Resolve<ServerPlayerService>();
            playerService.SpawnServerPlayer();
        }
        else if (mode == EMode.Client)
        {
            if (isMultiplayer)
            {
                var networkService = _gameScope.Resolve<ClientNetworkService>();
                networkService.Connect("127.0.0.1", 7777);
            }
        }
    }
    
    public void StopGame()
    {
        if (!IsGameRunning()) return;
        
        _graphicsDevice.WaitIdle();
        
        // TODO: dispatch a stop game event here to trigger other logic
        
        _gameScope?.Dispose();
        _gameScope = null;
        
        _updatables = _engineScope.RegisterUpdatables();
        _inputables = _engineScope.RegisterInputables();
        _drawables = _engineScope.RegisterDrawables();
        _guiDrawables = _engineScope.RegisterGuiDrawables();
                
        _logger.RefreshLoggers(_engineScope);
                
        GC.Collect();
    }
    
    public void Dispose()
    {
        _imGui.Dispose();
        GC.SuppressFinalize(this);
    }
}
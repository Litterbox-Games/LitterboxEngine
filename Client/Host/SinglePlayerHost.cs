using Autofac;
using Client.Core.Extensions;
using Client.Graphics;
using Common.Core;
using Common.Core.Attributes;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;

namespace Client.Host;

/// <summary>
///     The host used for single-player.
/// </summary>
public class SinglePlayerHost : IClientHost, IServerHost
{
    public EGameMode GameMode => EGameMode.SinglePlayer;
    public ILifetimeScope GameScope { get; set; } = null!;
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IGuiDrawable> GameGuiDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];

    public void Start(ILifetimeScope engineScope)
    {
        GameScope = engineScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterGameServices(EMode.Host, false);
        });
        
        GameScope.Resolve<RootLoggingService>().RefreshLoggers(GameScope);
        
        GameUpdatables = GameScope.RegisterUpdatables();
        GameInputables = GameScope.RegisterInputables();
        GameDrawables = GameScope.RegisterDrawables();
        GameGuiDrawables = GameScope.RegisterGuiDrawables();
        
        (this as IServerHost).SpawnServerPlayer();
    }

    public void Stop()
    {
        GameScope.Dispose();
        GameUpdatables = [];
        GameInputables = [];
        GameDrawables = [];
        GameGuiDrawables = [];
    }

    public void Dispose()
    { 
        GC.SuppressFinalize(this);
    }
}
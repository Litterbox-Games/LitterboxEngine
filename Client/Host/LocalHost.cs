using Autofac;
using Client.Core.Extensions;
using Client.Graphics;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;

namespace Client.Host;

/// <summary>
///     The host used for local hosting.
/// </summary>
public class LocalHost : IClientHost, IServerHost
{
    public EGameMode GameMode => EGameMode.LocalHost;
    public ILifetimeScope GameScope { get; set; } = null!;
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];

    public void Start(ILifetimeScope engineScope)
    {
        GameScope = engineScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(this)
                .As<IClientHost>()
                .As<IServerHost>()
                .As<IHost>()
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterServices(GameMode, ELifetime.Game);
        });
        
        GameScope.Resolve<RootLoggingService>().RefreshLoggers(GameScope);
        
        GameUpdatables = GameScope.RegisterUpdatables();
        GameInputables = GameScope.RegisterInputables();
        GameDrawables = GameScope.RegisterDrawables();
        
        (this as IServerHost).StartServer(7777);
        (this as IServerHost).SpawnServerPlayer();
    }

    public void Stop()
    {
        (this as IServerHost).StopServer();
        GameScope.Dispose();
        GameUpdatables = [];
        GameInputables = [];
        GameDrawables = [];
    }

    public void Dispose()
    { 
        GC.SuppressFinalize(this);
    }
}
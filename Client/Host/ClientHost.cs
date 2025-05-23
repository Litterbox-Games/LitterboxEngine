using Autofac;
using Client.Core.Extensions;
using Client.Graphics;
using Client.Services.Network;
using Common.Core;
using Common.Host;
using Common.Core.Extensions;
using Common.Services.Logging;

namespace Client.Host;

/// <summary>
///     The host used to represent the client game state.
/// </summary>
public class ClientHost : IClientHost
{
    public EGameMode GameMode => EGameMode.Client;
    
    public ILifetimeScope GameScope  { get; set; } = null!;

    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];

    public void Start(ILifetimeScope engineScope)
    {
        GameScope = engineScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterServices(GameMode, ELifetime.Game);
        });

        GameScope.Resolve<RootLoggingService>().RefreshLoggers(GameScope);
        
        GameUpdatables = GameScope.RegisterUpdatables();
        GameInputables = GameScope.RegisterInputables();
        GameDrawables = GameScope.RegisterDrawables();

        var networkService = GameScope.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
    }

    public void Stop()
    {
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
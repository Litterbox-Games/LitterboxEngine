using Arch.Core.Utils;
using Client.Graphics;
using Client.Services.Entities;
using Client.Services.Network;
using Client.Services.Players;
using Client.Services.Resource;
using Client.Services.World;
using Client.Systems;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Services.Entities;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Resource;
using Common.Services.World;
using Common.Systems;

namespace Client.Core.Registrars;

/// <summary>
///     Registers all client services.
/// </summary>
[RegistrarMode(EGameMode.Client), RegistrarPriority(EPriority.High)]
public class ClientServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        // Components
        ComponentRegistry.Add<Position>();
        
        // Services
        container.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        container.RegisterSingleton<EventService, EventService>();
        
        container.RegisterSingleton<IResourceService, ClientResourceService>();

        container.RegisterSingleton<NetworkService, ClientNetworkService>();
        container.RegisterSingleton<IPlayerService, ClientPlayerService>();
        container.RegisterSingleton<IEntityService, ClientEntityService>();
        container.RegisterSingleton<IWorldService, ClientWorldService>();
        
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
        
        // Systems
        container.RegisterSingleton<CameraSystem, CameraSystem>();
        container.RegisterSingleton<PlayerControlSystem, PlayerControlSystem>();
        container.RegisterSingleton<EntityRenderSystem, EntityRenderSystem>();
        container.RegisterSingleton<MovementSystem, MovementSystem>();
    }
}
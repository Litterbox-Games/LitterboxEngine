using Arch.Core.Utils;
using Client.Entities;
using Client.Entities.Systems;
using Client.Graphics;
using Client.Graphics.Input;
using Client.Network;
using Client.Players;
using Client.Resource;
using Client.World;
using Common.DI;
using Common.DI.Attributes;
using Common.Entities;
using Common.Entities.Components;
using Common.Host;
using Common.Logging;
using Common.Players;
using Common.Resource;
using Common.World;

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
        
        container.RegisterSingleton<IResourceService, ClientResourceService>();
        container.RegisterSingleton<InputService, InputService>();
        container.RegisterSingleton<CameraService, CameraService>();

        container.RegisterSingleton<IClientNetworkService, ClientNetworkService>();
        container.RegisterSingleton<IPlayerService, ClientPlayerService>();
        container.RegisterSingleton<IEntityService, ClientEntityService>();
        container.RegisterSingleton<IWorldService, ClientWorldService>();
        
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
        
        // Systems
        container.RegisterSingleton<PlayerControlSystem, PlayerControlSystem>();
        container.RegisterSingleton<EntityRenderSystem, EntityRenderSystem>();
        container.RegisterSingleton<ClientMovementSystem, ClientMovementSystem>();
    }
}
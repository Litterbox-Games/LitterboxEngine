using Arch.Core.Utils;
using Client.Entities.Systems;
using Client.Graphics;
using Client.Graphics.Input;
using Client.Resource;
using Client.World;
using Common.DI;
using Common.DI.Attributes;
using Common.Entities;
using Common.Entities.Components;
using Common.Entities.Systems;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Players;
using Common.Resource;
using Common.World;
using Common.World.Generation;

namespace Client.Core.Registrars;

/// <summary>
///     Registers all services for a host or single player application.
/// </summary>
[RegistrarMode(EGameMode.Host | EGameMode.SinglePlayer), RegistrarPriority(EPriority.High)]
public class HostServiceRegistrar : IServiceRegistrar
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
        
        container.RegisterSingleton<IServerNetworkService, ServerNetworkService>();
        container.RegisterSingleton<IPlayerService, ServerPlayerService>();
        container.RegisterSingleton<IEntityService, ServerEntityService>();
        container.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        container.RegisterSingleton<IWorldService, ServerWorldService>();
        
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
        
        // Systems
        container.RegisterSingleton<MobControllerSystem, MobControllerSystem>();
        container.RegisterSingleton<PlayerControlSystem, PlayerControlSystem>();
        container.RegisterSingleton<EntityRenderSystem, EntityRenderSystem>();
        container.RegisterSingleton<ServerMovementSystem, ServerMovementSystem>();        
    }
}
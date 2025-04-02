using Arch.Core.Utils;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Services.Block;
using Common.Services.Entities;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Resource;
using Common.Services.World;
using Common.Services.World.Generation;
using Common.Systems;

namespace Server.Host.Registrars;

/// <summary>
///     Registers all dedicated server services.
/// </summary>
[RegistrarMode(EGameMode.Dedicated), RegistrarPriority(EPriority.High)]
public class ServerServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        // Components
        ComponentRegistry.Add<Position>();
        
        // Services
        container.RegisterSingleton<BlockRegistry, BlockRegistry>();
        
        container.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        container.RegisterSingleton<IResourceService, ServerResourceService>();
        
        container.RegisterSingleton<EventService, EventService>();
        container.RegisterSingleton<NetworkService, ServerNetworkService>();
        container.RegisterSingleton<IPlayerService, ServerPlayerService>();
        container.RegisterSingleton<IEntityService, ServerEntityService>();
        container.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        container.RegisterSingleton<IWorldService, ServerWorldService>();
        
        // Systems
        container.RegisterSingleton<MobControllerSystem, MobControllerSystem>();
        container.RegisterSingleton<MovementSystem, MovementSystem>();
    }
}
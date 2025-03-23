using Arch.Core.Utils;
using Client.Graphics;
using Client.Graphics.Input;
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
using Common.Services.Logging;
using Common.Services.Players;
using Common.Services.Resource;
using Common.Services.World;

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
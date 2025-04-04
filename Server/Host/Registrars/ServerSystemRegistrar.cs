using Arch.Core.Utils;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Systems;

namespace Server.Host.Registrars;

/// <summary>
///     Registers any game systems for the dedicated host only.
/// </summary>
[RegistrarMode(EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.Low)]
public class ServerSystemRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        // Components
        // TODO: better way of registering components, they should be order independent so we probably don't need to register them explicitly
        ComponentRegistry.Add<Position>(); 
        
        // Systems
        container.RegisterSingleton<MobControllerSystem, MobControllerSystem>();
        container.RegisterSingleton<MovementSystem, MovementSystem>();
    }                                                                 
}
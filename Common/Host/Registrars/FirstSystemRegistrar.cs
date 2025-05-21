using Arch.Core.Utils;
using Autofac;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Systems;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any systems that all hosts share.
/// </summary>
[RegistrarMode(EGameMode.Client | EGameMode.Host | EGameMode.SinglePlayer | EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.Low)]
public class FirstSystemRegistrar: IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(ContainerBuilder container)
    {
        // Components
        // TODO: better way of registering components, they should be order independent so we probably don't need to register them explicitly
        ComponentRegistry.Add<Position>(); 
        
        // Systems
        container.RegisterType<MovementSystem>().AsSelf().SingleInstance();
    }    
}
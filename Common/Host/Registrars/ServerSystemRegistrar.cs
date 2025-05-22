using Autofac;
using Common.Core;
using Common.Core.Attributes;
using Common.Services.Entities;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any game systems for the dedicated host only.
/// </summary>
[RegistrarMode(EGameMode.Dedicated | EGameMode.Host | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.Low)]
public class ServerSystemRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(ContainerBuilder container)
    {
        // Systems
        container.RegisterType<MobControllerService>().AsSelf().SingleInstance();
    }                                                                 
}
using Autofac;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Services.Resource;
using Server.Services.Resource;

namespace Server.Host.Registrars;

/// <summary>
///     Registers any engine services for the dedicated host only.
/// </summary>
[RegistrarMode(EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Engine)]
[RegistrarPriority(EPriority.High)]
public class EngineRegistrar: IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(ContainerBuilder container)
    {
        container.RegisterType<ServerResourceService>().As<IResourceService>().AsSelf().SingleInstance();
    }    
}
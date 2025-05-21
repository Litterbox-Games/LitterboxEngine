using Autofac;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Client.Services.Entities;
using Client.Services.Network;
using Client.Services.Players;
using Client.Services.World;
using Common.Services.Block;
using Common.Services.Entities;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.World;

namespace Client.Host.Registrars;

/// <summary>
///     Registers any game services for the server-client host only.
/// </summary>
[RegistrarMode(EGameMode.Client)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.High)]
public class ClientGameRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(ContainerBuilder container)
    {
        container.RegisterType<BlockRegistry>().AsSelf().SingleInstance();
        
        container.RegisterType<ClientNetworkService>().As<NetworkService>().SingleInstance();
        container.RegisterType<ClientPlayerService>().As<IPlayerService>().SingleInstance();
        container.RegisterType<ClientEntityService>().As<IEntityService>().SingleInstance();
        container.RegisterType<ClientWorldService>().As<IWorldService>().SingleInstance();
    }                                                                 
}
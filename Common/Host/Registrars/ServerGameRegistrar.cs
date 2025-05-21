using Autofac;
using Common.Core;
using Common.Core.Attributes;
using Common.Services.Block;
using Common.Services.Entities;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.World;
using Common.Services.World.Generation;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any game services all server hosts share.
/// </summary>
[RegistrarMode(EGameMode.Dedicated | EGameMode.Host | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.High)]
public class ServerGameRegistrar: IServiceRegistrar
{
    public void RegisterServices(ContainerBuilder container)
    {
        container.RegisterType<BlockRegistry>().AsSelf().SingleInstance();
        
        container.RegisterType<ServerNetworkService>().As<NetworkService>().SingleInstance();
        container.RegisterType<ServerPlayerService>().As<IPlayerService>().SingleInstance();
        container.RegisterType<ServerEntityService>().As<IEntityService>().SingleInstance();
        container.RegisterType<EarthGenerator>().As<IWorldGenerator>().SingleInstance();
        container.RegisterType<ServerWorldService>().As<IWorldService>().SingleInstance();
    }
}
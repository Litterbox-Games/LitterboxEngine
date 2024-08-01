using System.Numerics;
using Common.Entity;
using Common.Host;
using Common.Network;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : AbstractHost
{
    /// <inheritdoc />
    public ServerHost() : base(EGameMode.Dedicated)
    {
        RegisterServices();
        
        var networking = (ServerNetworkService)Resolve<INetworkService>();

        const ushort port = 7777;
        
        networking.Listen(port);
        
        Resolve<MobControllerService>().SpawnMobEntity(new Vector2(5, 5));
        Resolve<MobControllerService>().SpawnMobEntity(new Vector2(-5, -5));
    }
}
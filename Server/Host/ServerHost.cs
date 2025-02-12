using System.Numerics;
using Common.Entity;
using Common.Host;
using Common.Network;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : BaseHost
{
    public ServerHost(): base(EGameMode.Dedicated)
    {
        var networking = Container.Resolve<ServerNetworkService>();

        const ushort port = 7777;
        
        networking.Listen(port);

        var mobController = Container.Resolve<MobControllerService>();
        for (var x = 0; x < 30; x++)
        {
            for (var y = 0; y < 30; y++)
            {
                mobController.SpawnMobEntity(new Vector2(x * 2, y * 2));
            }    
        }
    }
}
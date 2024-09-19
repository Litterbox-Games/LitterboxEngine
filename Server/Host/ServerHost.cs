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
        
        var networking = Resolve<ServerNetworkService>();

        const ushort port = 7777;
        
        networking.Listen(port);

        for (var x = 0; x < 30; x++)
        {
            for (var y = 0; y < 30; y++)
            {
                Resolve<MobControllerService>().SpawnMobEntity(new Vector2(x * 2, y * 2));
            }    
        }
    }
}
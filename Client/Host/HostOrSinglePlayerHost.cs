using System.Numerics;
using Common.Entity;
using Common.Host;
using Common.Network;

namespace Client.Host;

/// <summary>
///     The host used for single player or for local hosting.
/// </summary>
public class HostOrSinglePlayerHost : AbstractHost
{
    /// <inheritdoc />
    public HostOrSinglePlayerHost(bool singlePlayer) : base(singlePlayer ? EGameMode.SinglePlayer : EGameMode.Host)
    {
        RegisterServices();

        if (GameMode != EGameMode.Host)
            return;
        
        var networkService = Resolve<ServerNetworkService>();
        networkService.Listen(7777);
        
        for (var x = 0; x < 30; x++)
        {
            for (var y = 0; y < 30; y++)
            {
                Resolve<MobControllerService>().SpawnMobEntity(new Vector2(x * 2, y * 2));
            }    
        }
    }
}
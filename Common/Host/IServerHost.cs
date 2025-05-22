using System.Numerics;
using Autofac;
using Common.Services.Entities;
using Common.Services.Network;
using Common.Services.Players;

namespace Common.Host;

public interface IServerHost: IHost
{
    public void StartServer(ushort port)
    {
        var networking = GameContainer.Resolve<ServerNetworkService>();
        networking.Listen(port);

        var mobController = GameContainer.Resolve<MobControllerService>();
        for (var x = 0; x < 30; x++)
        {
            for (var y = 0; y < 30; y++)
            {
                mobController.SpawnMobEntity(new Vector2(x * 2, y * 2));
            }    
        }
    }
    
    public void StopServer()
    {
        var networking = GameContainer.Resolve<ServerNetworkService>();
        networking.StopListening();
    }

    public void SpawnServerPlayer()
    {
        var playerService = GameContainer.Resolve<ServerPlayerService>();
        playerService.SpawnServerPlayer();
    }
}
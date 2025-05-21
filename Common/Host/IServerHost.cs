using System.Numerics;
using Autofac;
using Common.Services.Network;
using Common.Services.Players;
using Common.Systems;

namespace Common.Host;

public interface IServerHost: IHost
{
    public void StartServer(ushort port)
    {
        if (GameContainer == null)
        {
            Console.WriteLine("GameContainer is null");
            return;
        }
        
        var networking = GameContainer.Resolve<ServerNetworkService>();
        networking.Listen(port);

        var mobController = GameContainer.Resolve<MobControllerSystem>();
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
        if (GameContainer == null)
        {
            Console.WriteLine("GameContainer is null");
            return;
        }
        
        var networking = GameContainer.Resolve<ServerNetworkService>();
        networking.StopListening();
    }

    public void SpawnServerPlayer()
    {
        if (GameContainer == null)
        {
            Console.WriteLine("GameContainer is null");
            return;
        }
        
        var playerService = GameContainer.Resolve<ServerPlayerService>();
        playerService.SpawnServerPlayer();
    }
}
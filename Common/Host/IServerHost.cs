using System.Numerics;
using Common.Entity;
using Common.Network;

namespace Common.Host;

public interface IServerHost: IHost
{
    public void StartServer(ushort port)
    {
        var networking = Container.Resolve<IServerNetworkService>();
        
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
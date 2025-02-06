using System.Numerics;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Network;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : IServerHost
{
    public Container Container { get; }
    private readonly List<(EPriority, ITickableService)> _tickables = [];
    
    public ServerHost()
    {
        Container = new Container(EGameMode.Dedicated);
        Container.RegisterServices();
        
        Container.FilterRegistries<ITickableService>((tickable, type) =>
        {
            var tickableAttribute =
                type.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(TickablePriorityAttribute));

            var priority = EPriority.Normal;

            if (tickableAttribute != null)
                priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

            var inserted = false;

            for (var i = 0; i < _tickables.Count && !inserted; i++)
            {
                if (priority <= _tickables[i].Item1)
                    continue;

                _tickables.Insert(i, (priority, tickable));
                inserted = true;
            }

            if (!inserted)
            {
                _tickables.Add((priority, tickable));
            }
        });
        
        var networking = Container.Resolve<ServerNetworkService>();

        const ushort port = 7777;
        
        networking.Listen(port);

        for (var x = 0; x < 30; x++)
        {
            for (var y = 0; y < 30; y++)
            {
                Container.Resolve<MobControllerService>().SpawnMobEntity(new Vector2(x * 2, y * 2));
            }    
        }
    }

    public void Update(float deltaTime)
    {
        _tickables.ForEach(x => x.Item2.Update(deltaTime));
    }
    
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}
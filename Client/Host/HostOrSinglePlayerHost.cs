using System.Numerics;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Network;
using Common.Player;

namespace Client.Host;

/// <summary>
///     The host used for single player or for local hosting.
/// </summary>
public class HostOrSinglePlayerHost : IClientHost
{
    public Container Container { get; }
    private readonly List<(EPriority, ITickableService)> _tickables = [];
    
    public HostOrSinglePlayerHost(bool singlePlayer)
    {
        Container = new Container(singlePlayer ? EGameMode.SinglePlayer : EGameMode.Host);
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
        
        // Warm Service Singletons
        Container.Resolve<IPlayerService>();

        var networkService = Container.Resolve<ServerNetworkService>();
        networkService.Listen(7777);
        
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
    
    public void Draw()
    {
        _tickables.ForEach(x => x.Item2.Draw());
    }
    
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}
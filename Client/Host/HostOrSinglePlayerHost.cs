using System.Numerics;
using Client.Graphics;
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
    private readonly List<(EPriority, IUpdatable)> _updatables = [];
    private readonly List<IDrawable> _drawables = [];
    
    public HostOrSinglePlayerHost(bool singlePlayer)
    {
        Container = new Container(singlePlayer ? EGameMode.SinglePlayer : EGameMode.Host);
        Container.RegisterServices();
        
        Container.FilterRegistries<IUpdatable>((updatable, type) =>
        {
            var tickableAttribute =
                type.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(TickablePriorityAttribute));

            var priority = EPriority.Normal;

            if (tickableAttribute != null)
                priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

            var inserted = false;

            for (var i = 0; i < _updatables.Count && !inserted; i++)
            {
                if (priority <= _updatables[i].Item1)
                    continue;

                _updatables.Insert(i, (priority, updatable));
                inserted = true;
            }

            if (!inserted)
            {
                _updatables.Add((priority, updatable));
            }
        });
        
        Container.FilterRegistries<IDrawable>((drawable, _) =>
        {
            _drawables.Add(drawable);
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
        _updatables.ForEach(x => x.Item2.Update(deltaTime));
    }
    
    public void Draw(Renderer renderer)
    {
        _drawables.ForEach(drawable => drawable.Draw(renderer));
    }
    
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}
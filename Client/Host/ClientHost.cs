using Client.Graphics;
using Client.Network;
using Common.DI;
using Common.DI.Attributes;
using Common.Host;
using Common.Player;

namespace Client.Host;

/// <summary>
///     the host used to represent the client game state.
/// </summary>
public class ClientHost : IClientHost
{
    public Container Container { get; }
    private readonly List<(EPriority, IUpdatable)> _updatables = [];
    private readonly List<IDrawable> _drawables = [];
    
    public ClientHost()
    {
        Container = new Container(EGameMode.Client);
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
        
        var networkService = Container.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
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
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
    private readonly List<(EPriority, ITickableService)> _tickables = [];
    
    public ClientHost()
    {
        Container = new Container(EGameMode.Client);
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
        
        var networkService = Container.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
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
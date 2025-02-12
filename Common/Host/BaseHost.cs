using Common.DI;
using Common.DI.Attributes;

namespace Common.Host;

public class BaseHost: IUpdatable, IDisposable
{
    public readonly Container Container;
    private readonly List<(EPriority, IUpdatable)> _updatables = [];
    
    public BaseHost(EGameMode gameMode)
    {
        Container = new Container(gameMode);
        Container.RegisterServices();
        
        Container.FilterRegistries<IUpdatable>((updatable, type) =>
        {
            var tickableAttribute =
                type.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(UpdatablePriorityAttribute));

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
    }

    public void Update(float deltaTime)
    {
        _updatables.ForEach(x => x.Item2.Update(deltaTime));
    }
    
    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}
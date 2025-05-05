using System.Runtime.CompilerServices;
using Common.Core;
using Common.Core.Attributes;

[assembly: InternalsVisibleTo("Client")]

namespace Common.Host;

public interface IHost: IDisposable
{
    public IContainer EngineContainer { get; }
    public IContainer? GameContainer { get; set; }
    
    List<(EPriority, IUpdatable)> EngineUpdatables { get; }
    List<(EPriority, IUpdatable)> GameUpdatables { get; }

    public List<(EPriority, IUpdatable)> RegisterUpdatables(IContainer container)
    {
        var updatables = new List<(EPriority, IUpdatable)>(); 
        
        container.FilterRegistrations<IUpdatable>((updatable, type) =>
        {
            var tickableAttribute =
                type.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(UpdatablePriorityAttribute));

            var priority = EPriority.Normal;

            if (tickableAttribute != null)
                priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

            var inserted = false;

            for (var i = 0; i < updatables.Count && !inserted; i++)
            {
                if (priority <= updatables[i].Item1)
                    continue;

                updatables.Insert(i, (priority, updatable));
                inserted = true;
            }

            if (!inserted)
            {
                updatables.Add((priority, updatable));
            }
        });

        return updatables;
    }

    public void Start(EGameMode gameMode);
    
    public void Stop();
    
    public void Update(float deltaTime);
}
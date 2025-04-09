using System.Runtime.CompilerServices;
using Common.Core;
using Common.Core.Attributes;

[assembly: InternalsVisibleTo("Client")]

namespace Common.Host;

public interface IHost: IDisposable
{
    public IContainer Container { get; }
    List<(EPriority, IUpdatable)> Updatables { get; }

    public void RegisterUpdatables()
    {
        Container.FilterRegistries<IUpdatable>((updatable, type) =>
        {
            var tickableAttribute =
                type.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(UpdatablePriorityAttribute));

            var priority = EPriority.Normal;

            if (tickableAttribute != null)
                priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

            var inserted = false;

            for (var i = 0; i < Updatables.Count && !inserted; i++)
            {
                if (priority <= Updatables[i].Item1)
                    continue;

                Updatables.Insert(i, (priority, updatable));
                inserted = true;
            }

            if (!inserted)
            {
                Updatables.Add((priority, updatable));
            }
        });
    }

    public void Update(float deltaTime)
    {
        Updatables.ForEach(x => x.Item2.Update(deltaTime));
    }
}
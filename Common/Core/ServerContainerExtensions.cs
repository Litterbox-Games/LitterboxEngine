using Common.Core.Attributes;

namespace Common.Core;

public static class ServerContainerExtensions
{
    public static List<(EPriority, IUpdatable)> RegisterUpdatables(this IContainer container )
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
}
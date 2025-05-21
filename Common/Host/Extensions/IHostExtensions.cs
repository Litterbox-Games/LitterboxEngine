using Common.Core;
using Common.Core.Attributes;
using Common.Core.Extensions;

namespace Common.Host.Extensions;

public static class IHostExtensions
{
    public static void RegisterUpdatables(this IHost host)
    {
        host.GameContainer!.FilterRegistrations<IUpdatable>((updatable, type) =>
        {
            var tickableAttribute =
                type.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(UpdatablePriorityAttribute));

            var priority = EPriority.Normal;

            if (tickableAttribute != null)
                priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

            var inserted = false;

            for (var i = 0; i < host.GameUpdatables.Count && !inserted; i++)
            {
                if (priority <= host.GameUpdatables[i].Item1)
                    continue;

                host.GameUpdatables.Insert(i, (priority, updatable));
                inserted = true;
            }

            if (!inserted)
            {
                host.GameUpdatables.Add((priority, updatable));
            }
        });
    }
}
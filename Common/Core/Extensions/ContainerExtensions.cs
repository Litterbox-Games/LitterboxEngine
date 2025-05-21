using Autofac;
using Autofac.Core;
using Common.Core.Attributes;
using MoreLinq;

namespace Common.Core.Extensions;

public static class ContainerExtensions
{
    public static void FilterRegistrations<T>(this IContainer container, Action<T, Type> action)
    {
        container.ComponentRegistry.Registrations.SelectMany(x => x.Services)
            .OfType<IServiceWithType>()
            .Select(x => x.ServiceType)
            .Where(x => x.IsAssignableTo(typeof(T)))
            .ForEach(x =>
            {
                var service = (T)container.Resolve(x);
                action(service, service!.GetType());
            });
    }
    
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
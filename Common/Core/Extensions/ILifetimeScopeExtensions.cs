using System.Reflection;
using Autofac;
using Autofac.Core;
using MoreLinq;

namespace Common.Core.Extensions;

public static class ILifetimeScopeExtensions
{
    public static void FilterRegistrations<T>(this ILifetimeScope container, Action<T, Type> action)
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
    
    public static List<(float, IUpdatable)> RegisterUpdatables(this ILifetimeScope container)
    {
        var updatables = new List<(float, IUpdatable)>();

        container.FilterRegistrations<IUpdatable>((updatable, type) =>
        {
            var updateMethod = type.GetMethod(nameof(IUpdatable.Update), BindingFlags.Instance | BindingFlags.Public);

            var priorityAttribute = updateMethod?
                .GetCustomAttributes(typeof(PriorityAttribute), inherit: true)
                .Cast<PriorityAttribute>()
                .FirstOrDefault();

            var priority = priorityAttribute?.Priority ?? (float)EPriority.Normal;

            updatables.Add((priority, updatable));
        });

        // Sort descending by priority (highest -> lowest)
        updatables.Sort((a, b) => b.Item1.CompareTo(a.Item1));

        return updatables;
    }
}
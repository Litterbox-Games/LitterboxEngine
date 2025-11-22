using System.Reflection;
using Autofac;
using Autofac.Core;
using MoreLinq;

namespace Common.Core.Extensions;

public static class ILifetimeScopeExtensions
{
    public static void FilterRegistrations<T>(this ILifetimeScope container, Action<T, Type> action)
    {
        container
            .GetRegistrationsRecursive()
            .SelectMany(x => x.Services)
            .OfType<IServiceWithType>()
            .Select(x => x.ServiceType)
            .Where(x => x.IsAssignableTo(typeof(T)))
            .ForEach(x =>
            {
                var service = (T)container.Resolve(x);
                action(service, service!.GetType());
            });
    }
    
    private static IEnumerable<IComponentRegistration> GetRegistrationsRecursive(this ILifetimeScope scope)
    {
        if (scope is not ISharingLifetimeScope)
            return scope.ComponentRegistry.Registrations;
        
        var current = scope as ISharingLifetimeScope;

        var registrations = new List<IComponentRegistration>();
        while (current != null)
        {
            registrations.AddRange(current.ComponentRegistry.Registrations);
            current = current.ParentLifetimeScope;
        }
        
        return registrations;
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
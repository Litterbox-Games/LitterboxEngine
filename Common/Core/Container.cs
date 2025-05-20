using Common.Core.Attributes;
using Common.Core.Exceptions;
using Common.Host;
using MoreLinq.Extensions;
using Unity;
using Unity.Lifetime;

namespace Common.Core;

public sealed class Container(IUnityContainer? container = null): IContainer
{
    private readonly IUnityContainer _container = container ?? new UnityContainer();
    
    public void RegisterServices(EGameMode gameMode, ELifetime lifetime)
    {
        RegisterSingleton<IContainer, Container>(this, false);
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var registrars = assemblies.SelectMany(assembly =>
        {
            return assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IServiceRegistrar)) && type != typeof(IServiceRegistrar));
        }).ToArray();

        var types = new List<(EPriority, Type)>(registrars.Length);
        
        registrars.ForEach(type =>
        {
            var attributes = type.CustomAttributes;

            var doRegister = true;
            var registrarPriority = EPriority.Normal;
            
            attributes.ForEach( x =>
            {
                if (x.AttributeType == typeof(RegistrarIgnoreAttribute))
                {
                    doRegister = false;
                    return;
                }

                if (x.AttributeType == typeof(RegistrarLifetimeAttribute))
                {
                    var registrarLifetime = (ELifetime)x.ConstructorArguments[0].Value!;

                    if (!registrarLifetime.HasFlag(lifetime))
                    {
                        doRegister = false;
                        return;
                    }
                }
                
                if (x.AttributeType == typeof(RegistrarModeAttribute))
                {
                    var registrarMode = (EGameMode)x.ConstructorArguments[0].Value!;

                    if (!registrarMode.HasFlag(gameMode))
                    {
                        doRegister = false;
                        return;
                    }
                }
                
                if (x.AttributeType == typeof(RegistrarPriorityAttribute))
                {
                    registrarPriority = (EPriority)x.ConstructorArguments[0].Value!;
                }
            });

            if (doRegister)
                types.Add((registrarPriority, type));
        });

        var sortedTypes = types.OrderBy(priority => (int)priority.Item1).Reverse();
        
        sortedTypes.ForEach(x =>
        {
            var registrar = (IServiceRegistrar)Activator.CreateInstance(x.Item2)!;
            registrar.RegisterServices(this);
        });
    }

    public IContainer CreateChildContainer()
    {
        var child = _container.CreateChildContainer();
        
        // _container.Registrations.Where(x => x.MappedToType.IsAssignableTo(typeof(IService))).ForEach((registration) =>
        // {
        //     // child.RegisterInstance(registration.MappedToType, registration, new ExternallyControlledLifetimeManager());
        // });
        
        /*
        _container.Registrations
            .Where(x => x.MappedToType.IsAssignableTo(typeof(IService)) && !x.MappedToType.IsAssignableTo(typeof(IContainer)))
            .ForEach(registration =>
            {
                var service = _container.Resolve(registration.MappedToType);
                
                child.RegisterSingleton(registration.MappedToType);
                child.RegisterInstance(registration.RegisteredType, registration.Name, service, new ExternallyControlledLifetimeManager());
            });
        */
        
        return new Container(child);
    }

    public void FilterRegistrations<T>(Action<T, Type> action)
    {
        _container.Registrations
            .Where(x => x.MappedToType.IsAssignableTo(typeof(T)))
            .ForEach(registration =>
            {
                var service = (T)_container.Resolve(registration.MappedToType);
                action(service, registration.RegisteredType);
            });
    }
    
    /// <inheritdoc />
    public void RegisterSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        _container.RegisterSingleton<TInstance>();
        _container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.ContainerControlled);
    }
    
    /// <inheritdoc />
    public void RegisterTransient<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        // TODO: how should we handle this now that the Container is unaware of ITickableService?
        // if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)))
        //     throw new NotImplementedException("Anything implementing ITickableService does not currently support transient lifetimes.");
        
        _container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.Transient);
    }
    
    /// <inheritdoc />
    public void RegisterThreadedSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        // TODO: how should we handle this now that the Container is unaware of ITickableService?
        // if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)))
        //     throw new NotImplementedException("Anything implementing ITickableService does not currently support threaded lifetimes.");
        
        _container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.PerThread);
    }

    /// <inheritdoc />
    public void RegisterSingleton<TContract, TInstance>(TInstance instance, bool performBuildup, string? mapping = null) where TInstance : TContract where TContract : IService
    {
        _container.RegisterSingleton<TInstance>(mapping);
        _container.RegisterInstance<TContract>(mapping, instance);
    }

    /// <inheritdoc />
    public T Resolve<T>(string? mapping = null) where T : IService
    {
        if (mapping == null)
        {
            if (!_container.IsRegistered<T>())
                throw new ContainerResolveException(typeof(T), mapping);
        }
        else
        {
            if (!_container.IsRegistered<T>(mapping))
                throw new ContainerResolveException(typeof(T), mapping);
        }
        
        return mapping == null ? _container.Resolve<T>() : _container.Resolve<T>(mapping);
    }

    /// <inheritdoc />
    public IEnumerable<T> ResolveAll<T>() where T : IService
    {
        return _container.ResolveAll<T>();
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        _container.Dispose();
    }
}
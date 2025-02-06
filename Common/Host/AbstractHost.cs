using Common.DI;
using Common.DI.Attributes;
using Common.DI.Exceptions;
using Common.Host;
using MoreLinq.Extensions;
using Unity;

public sealed class Container(EGameMode gameMode) : IContainer
{
    private readonly UnityContainer _container = new();
    
    public EGameMode GameMode { get; } = gameMode;

    public void FilterRegistries<T>(Action<T, Type> action) where T: IService
    {
        _container.Registrations
            .Where(x => x.MappedToType.IsAssignableTo(typeof(T)))
            .ForEach(registration =>
            {
                var service = (T)_container.Resolve(registration.MappedToType);
                action(service, registration.MappedToType);
            });
    }
    
    
    public void RegisterServices()
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

                if (x.AttributeType == typeof(RegistrarModeAttribute))
                {
                    var registrarMode = (EGameMode)x.ConstructorArguments[0].Value!;

                    if (!registrarMode.HasFlag(GameMode))
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
    
    /// <inheritdoc />
    public void RegisterSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        _container.RegisterSingleton<TInstance>();
        _container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.Singleton);
    }
    
    /// <inheritdoc />
    public void RegisterTransient<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)))
            throw new NotImplementedException("Anything implementing ITickableService does not currently support transient lifetimes.");
        
        _container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.Transient);
    }
    
    /// <inheritdoc />
    public void RegisterThreadedSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)))
            throw new NotImplementedException("Anything implementing ITickableService does not currently support threaded lifetimes.");
        
        _container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.PerThread);
    }

    /// <inheritdoc />
    public void RegisterSingleton<TContract, TInstance>(TInstance instance, bool performBuildup, string? mapping = null) where TInstance : TContract where TContract : IService
    {
        _container.RegisterSingleton<TInstance>();
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
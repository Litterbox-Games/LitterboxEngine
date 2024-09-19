using System;
using System.Collections.Generic;
using System.Linq;
using Common.DI.Attributes;
using Common.DI;
using Common.DI.Exceptions;
using MoreLinq;
using Unity;
using Unity.Lifetime;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Host;

// This class is created DURING the initialization of the game THROUGH the menu.
// When the player leaves a server, this host will be disposed and a new one will be created in its place.
public abstract class AbstractHost : IHost, IDisposable
{
    /// <inheritdoc />
    public EGameMode GameMode { get; }

    /// <inheritdoc />
    public UnityContainer Container { get; } = new();

    protected readonly List<(EPriority, ITickableService)> _tickables = new();

    private bool _registrationComplete;

    protected AbstractHost(EGameMode mode)
    {
        GameMode = mode;
    }

    protected void RegisterServices()
    {
        RegisterSingleton<IHost, AbstractHost>(this, false);
        
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
            
            attributes.ForEach(x =>
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

        var tickableTypes = Container.Registrations.Where(x => x.MappedToType.IsAssignableTo(typeof(ITickableService)));
        var tickables = new List<(EPriority, Type)>();

        tickableTypes.ForEach(x =>
        {
            var tickableAttribute = x.MappedToType.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(TickablePriorityAttribute));

            var priority = EPriority.Normal;
            
            if (tickableAttribute != null)
                priority = (EPriority)tickableAttribute.ConstructorArguments[0].Value!;
            
            tickables.Add((priority, x.MappedToType));
        });

        var sortedTickables = tickables.OrderBy(x => x.Item1).Reverse();
        
        sortedTickables.ForEach(x =>
        {
            var tickable = (ITickableService)Container.Resolve(x.Item2);
            
            _tickables.Add((x.Item1, tickable));
        });

        _registrationComplete = true;
    }
    
    /// <inheritdoc />
    public void RegisterSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        Container.RegisterSingleton<TInstance>();
        Container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.Singleton);

        if (!typeof(TInstance).IsAssignableTo(typeof(ITickableService)) || !_registrationComplete)
            return;
        
        var priority = EPriority.Normal;

        var tickableAttribute = typeof(TInstance).CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(TickablePriorityAttribute));

        if (tickableAttribute != null)
            priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

        var inserted = false;

        var tickable = (ITickableService) Container.Resolve(typeof(TInstance));

        for (var i = 0; i < _tickables.Count && !inserted; i++)
        {
            if (priority <= _tickables[i].Item1)
                continue;

            _tickables.Insert(i, (priority, tickable));
            inserted = true;
        }

        if (!inserted)
        {
            _tickables.Add((priority, tickable));
        }
    }
    
    /// <inheritdoc />
    public void RegisterTransient<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)))
            throw new NotImplementedException("Anything implementing ITickableService does not currently support transient lifetimes.");
        
        Container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.Transient);
    }
    
    /// <inheritdoc />
    public void RegisterThreadedSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService
    {
        if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)))
            throw new NotImplementedException("Anything implementing ITickableService does not currently support threaded lifetimes.");
        
        Container.RegisterType<TContract, TInstance>(mapping, TypeLifetime.PerThread);
    }

    /// <inheritdoc />
    public void RegisterSingleton<TContract, TInstance>(TInstance instance, bool performBuildup, string? mapping = null) where TInstance : TContract where TContract : IService
    {
        if (typeof(TInstance).IsAssignableTo(typeof(ITickableService)) && _registrationComplete)
        {
            var priority = EPriority.Normal;

            var tickableAttribute = typeof(TInstance).CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(TickablePriorityAttribute));

            if (tickableAttribute != null)
                priority = (EPriority) tickableAttribute.ConstructorArguments[0].Value!;

            var inserted = false;

            var tickable = (ITickableService) instance;

            for (var i = 0; i < _tickables.Count && !inserted; i++)
            {
                if (priority <= _tickables[i].Item1)
                    continue;

                _tickables.Insert(i, (priority, tickable));
                inserted = true;
            }

            if (!inserted)
            {
                _tickables.Add((priority, tickable));
            }

            if (performBuildup)
                Container.BuildUp(instance);
        }
        
        Container.RegisterSingleton<TInstance>();
        Container.RegisterInstance<TContract>(mapping, instance);
    }

    /// <inheritdoc />
    public T Resolve<T>(string? mapping = null) where T : IService
    {
        if (mapping == null)
        {
            if (!Container.IsRegistered<T>())
                throw new ContainerResolveException(typeof(T), mapping);
        }
        else
        {
            if (!Container.IsRegistered<T>(mapping))
                throw new ContainerResolveException(typeof(T), mapping);
        }
        
        return mapping == null ? Container.Resolve<T>() : Container.Resolve<T>(mapping);
    }

    /// <inheritdoc />
    public IEnumerable<T> ResolveAll<T>() where T : IService
    {
        return Container.ResolveAll<T>();
    }

    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        _tickables.ForEach(x => x.Item2.Update(deltaTime));
    }

    /// <inheritdoc />
    public void Draw()
    {
        _tickables.ForEach(x => x.Item2.Draw());
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}
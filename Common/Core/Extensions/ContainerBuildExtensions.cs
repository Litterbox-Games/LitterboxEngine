using Autofac;
using Common.Core.Attributes;
using Common.Host;
using MoreLinq;

namespace Common.Core;

public static class ContainerBuildExtensions
{
    public static void RegisterServices(this ContainerBuilder builder, EGameMode gameMode, ELifetime lifetime)
    {
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
            registrar.RegisterServices(builder);
        });
    }
}
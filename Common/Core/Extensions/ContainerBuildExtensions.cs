using Autofac;
using Autofac.Features.AttributeFilters;
using Common.Core.Attributes;
using Common.Host;
using MoreLinq;

namespace Common.Core.Extensions;

public static class ContainerBuildExtensions
{
    // public static ContainerBuilder RegisterServices(this ContainerBuilder builder, EGameMode gameMode, ELifetime lifetime)
    // {
    //     var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    //
    //     var registrars = assemblies.SelectMany(assembly =>
    //     {
    //         return assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IServiceRegistrar)) && type != typeof(IServiceRegistrar));
    //     }).ToArray();
    //
    //     var types = new List<(EPriority, Type)>(registrars.Length);
    //     
    //     registrars.ForEach(type =>
    //     {
    //         var attributes = type.CustomAttributes;
    //
    //         var doRegister = true;
    //         var registrarPriority = EPriority.Normal;
    //         
    //         attributes.ForEach( x =>
    //         {
    //             if (x.AttributeType == typeof(RegistrarIgnoreAttribute))
    //             {
    //                 doRegister = false;
    //                 return;
    //             }
    //
    //             if (x.AttributeType == typeof(RegistrarLifetimeAttribute))
    //             {
    //                 var registrarLifetime = (ELifetime)x.ConstructorArguments[0].Value!;
    //
    //                 if (!registrarLifetime.HasFlag(lifetime))
    //                 {
    //                     doRegister = false;
    //                     return;
    //                 }
    //             }
    //             
    //             if (x.AttributeType == typeof(RegistrarModeAttribute))
    //             {
    //                 var registrarMode = (EGameMode)x.ConstructorArguments[0].Value!;
    //
    //                 if (!registrarMode.HasFlag(gameMode))
    //                 {
    //                     doRegister = false;
    //                     return;
    //                 }
    //             }
    //             
    //             if (x.AttributeType == typeof(RegistrarPriorityAttribute))
    //             {
    //                 registrarPriority = (EPriority)x.ConstructorArguments[0].Value!;
    //             }
    //         });
    //
    //         if (doRegister)
    //             types.Add((registrarPriority, type));
    //     });
    //
    //     var sortedTypes = types.OrderBy(priority => (int)priority.Item1).Reverse();
    //     
    //     sortedTypes.ForEach(x =>
    //     {
    //         var registrar = (IServiceRegistrar)Activator.CreateInstance(x.Item2)!;
    //         registrar.RegisterServices(builder);
    //     });
    //
    //     return builder;
    // }

    public static ContainerBuilder RegisterEngineServices(this ContainerBuilder builder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var services = assemblies.SelectMany(assembly =>
            assembly.GetTypes().Where(
                type => type.IsClass && 
                !type.IsAbstract && 
                type.IsAssignableTo(typeof(IService)))
        ).ToArray();

        services.ForEach(service =>
        {
            var attributes = service.CustomAttributes;
            var hasEngineAttribute = attributes.Any(attribute => attribute.AttributeType == typeof(EngineAttribute));
            if (!hasEngineAttribute) return;

            var contracts = GetAsAttributes(service);
            var keyedContracts = contracts.Where(contract => contract.key is not null).ToArray();
            var unkeyedContracts = contracts.Where(contract => contract.key is null).Select(b => b.contract).ToArray();

            if (keyedContracts.Length > 0)
            {
                keyedContracts.ForEach(contract => {
                    var registration = builder.RegisterType(service).WithAttributeFiltering().Keyed(contract.key!, contract.contract);
                    unkeyedContracts.ForEach(unkeyed => registration.As(unkeyed));
                    registration.SingleInstance();
                });
            }
            else
            {
                var registration = builder.RegisterType(service).WithAttributeFiltering().AsSelf();
                unkeyedContracts.ForEach(contract => registration.As(contract));
                registration.SingleInstance();
            }
        });

        return builder;
    }

    public static ContainerBuilder RegisterGameServices(this ContainerBuilder builder, EMode mode, bool isMultiplayer)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var services = assemblies.SelectMany(assembly =>
            assembly.GetTypes().Where(
                type => type.IsClass && 
                !type.IsAbstract && 
                type.IsAssignableTo(typeof(IService)))
        ).ToArray();

        services.ForEach(service =>
        {
            var attributes = service.CustomAttributes;
            var gameAttribute = attributes.FirstOrDefault(attribute => attribute.AttributeType == typeof(GameAttribute));
            if (gameAttribute == null) return;

            var serviceModes = (EMode)gameAttribute.ConstructorArguments[0].Value!;
            if (!serviceModes.HasFlag(mode)) return;

            var serviceIsMultiplayer = attributes.Any(attribute => attribute.AttributeType == typeof(MultiplayerAttribute));
            if (!isMultiplayer && serviceIsMultiplayer) return;

            var contracts = GetAsAttributes(service);
            var keyedContracts = contracts.Where(contract => contract.key is not null).ToArray();
            var unkeyedContracts = contracts.Where(contract => contract.key is null).Select(b => b.contract).ToArray();

            if (keyedContracts.Length > 0)
            {
                keyedContracts.ForEach(contract => {
                    var registration = builder.RegisterType(service).WithAttributeFiltering().Keyed(contract.key!, contract.contract);
                    unkeyedContracts.ForEach(unkeyed => registration.As(unkeyed));
                    registration.SingleInstance();
                });
            }
            else
            {
                var registration = builder.RegisterType(service).WithAttributeFiltering().AsSelf();
                unkeyedContracts.ForEach(contract => registration.As(contract));
                registration.SingleInstance();
            }
        });

        return builder;
    }

    private static IEnumerable<(Type contract, object? key)> GetAsAttributes(Type type) =>
        type.CustomAttributes
            .Where(a => a.AttributeType.IsGenericType && a.AttributeType.GetGenericTypeDefinition() == typeof(AsAttribute<>))
            .Select(a => (a.AttributeType.GenericTypeArguments[0], a.ConstructorArguments.FirstOrDefault().Value));
}
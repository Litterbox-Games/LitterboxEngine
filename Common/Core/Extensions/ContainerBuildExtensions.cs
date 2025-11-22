using Autofac;
using Autofac.Features.AttributeFilters;
using MoreLinq;

namespace Common.Core.Extensions;

public static class ContainerBuildExtensions
{
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
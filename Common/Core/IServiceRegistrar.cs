using Autofac.Core;

namespace Common.Core;

/// <summary>
///     A service registrar used to handle the registration of specific services.
/// </summary>
public interface IServiceRegistrar
{
    /// <summary>
    ///     Registers all services for this registrar.
    /// </summary>
    void RegisterServices(Container container);
}
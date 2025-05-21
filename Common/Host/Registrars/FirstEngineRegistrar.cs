using Autofac;
using Common.Core;
using Common.Core.Attributes;
using Common.Services.Events;
using Common.Services.Logging;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any services that all hosts share.
/// </summary>
[RegistrarMode(EGameMode.Client | EGameMode.Host | EGameMode.SinglePlayer | EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Engine)]
[RegistrarPriority(EPriority.VeryHigh)]
public class FirstEngineRegistrar: IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(ContainerBuilder container)
    {
        container.RegisterType<EventService>().AsSelf().SingleInstance();
        
        //container.RegisterType<RootLoggingService>().As<ILoggingService>().SingleInstance();
        container.RegisterType<ConsoleLoggingService>().As<ILoggingService>().AsSelf().SingleInstance();
    }    
}
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
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        container.RegisterSingleton<EventService, EventService>();   
    }    
    
}
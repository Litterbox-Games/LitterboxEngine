using Common.Core;
using Common.Core.Attributes;
using Common.Services.Logging;

namespace Common.Host.Registrars;

/// <summary>
///     Registers an services that must be loaded last.
/// </summary>
[RegistrarPriority(EPriority.VeryLow)]
public class FinalServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<ILoggingService, RootLoggingService>();

        var logger = container.Resolve<ILoggingService>() as RootLoggingService;
        
        container.RegisterSingleton<ILoggingService, RootLoggingService>(logger!, false, "root");
    }
}
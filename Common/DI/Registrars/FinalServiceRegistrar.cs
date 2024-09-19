using Common.DI.Attributes;
using Common.Host;
using Common.Logging;

namespace Common.DI.Registrars;

/// <summary>
///     Registers an services that must be loaded last.
/// </summary>
[RegistrarPriority(EPriority.VeryLow)]
public class FinalServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(AbstractHost host)
    {
        host.RegisterSingleton<ILoggingService, RootLoggingService>();

        var logger = host.Resolve<ILoggingService>() as RootLoggingService;
        
        host.RegisterSingleton<ILoggingService, RootLoggingService>(logger!, false, "root");
    }
}
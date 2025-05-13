using Common.Core;
using Common.Core.Attributes;
using Common.Services.Logging;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any engine services that must be loaded last.
/// </summary>
[RegistrarPriority(EPriority.VeryLow)]
[RegistrarLifetime(ELifetime.Engine)]
public class FinalEngineRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<ILoggingService, RootLoggingService>();

        var logger = container.Resolve<ILoggingService>() as RootLoggingService;
        
        container.RegisterSingleton<ILoggingService, RootLoggingService>(logger!, false, "root");
    }
}
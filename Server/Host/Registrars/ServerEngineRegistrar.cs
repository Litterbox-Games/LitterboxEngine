using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Services.Resource;

namespace Server.Host.Registrars;

/// <summary>
///     Registers any engine services for the dedicated host only.
/// </summary>
[RegistrarMode(EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Engine)]
[RegistrarPriority(EPriority.High)]
public class ServerEngineRegistrar: IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<IResourceService, ServerResourceService>();
    }    
}
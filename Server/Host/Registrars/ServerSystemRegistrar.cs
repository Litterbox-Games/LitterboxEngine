using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Systems;

namespace Server.Host.Registrars;

/// <summary>
///     Registers any game systems for the dedicated host only.
/// </summary>
[RegistrarMode(EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.Low)]
public class ServerSystemRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        // Systems
        container.RegisterSingleton<MobControllerSystem, MobControllerSystem>();
    }                                                                 
}
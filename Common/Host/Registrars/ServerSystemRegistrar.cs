using Common.Core;
using Common.Core.Attributes;
using Common.Systems;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any game systems for the dedicated host only.
/// </summary>
[RegistrarMode(EGameMode.Dedicated | EGameMode.Host | EGameMode.SinglePlayer)]
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
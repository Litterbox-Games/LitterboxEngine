using Client.Entity;
using Client.Graphics;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;

namespace Client.DI.Registrars;

/// <summary>
///     Registers all services for a host or single player application.
/// </summary>
[RegistrarMode(EGameMode.Host | EGameMode.SinglePlayer), RegistrarPriority(EPriority.High)]
public class HostServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(AbstractHost host)
    {
        host.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        host.RegisterSingleton<CameraService, CameraService>();
        
        // Register Services Here
        host.RegisterSingleton<INetworkService, ServerNetworkService>();
        host.RegisterSingleton<IPlayerService, ServerPlayerService>();
        host.RegisterSingleton<IEntityService, ServerEntityService>();
        
        host.RegisterSingleton<EntityRenderService, EntityRenderService>();
    }
}
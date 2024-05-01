using System.ComponentModel.Design;
using Client.Entity;
using Client.Graphics;
using Client.Network;
using Client.Player;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;

namespace Client.DI.Registrars;

/// <summary>
///     Registers all client services.
/// </summary>
[RegistrarMode(EGameMode.Client), RegistrarPriority(EPriority.High)]
public class ClientServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(AbstractHost host)
    {
        host.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        host.RegisterSingleton<CameraMovementService, CameraMovementService>();
        
        host.RegisterSingleton<INetworkService, ClientNetworkService>();
        host.RegisterSingleton<IPlayerService, ClientPlayerService>();
        host.RegisterSingleton<IEntityService, ClientEntityService>();
        
        host.RegisterSingleton<EntityRenderService, EntityRenderService>();
    }
}
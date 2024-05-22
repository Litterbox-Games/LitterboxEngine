using Client.Entity;
using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.Input;
using Client.Resource;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;
using Common.Resource;

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
        host.RegisterSingleton<IWindowService, GlfwWindowService>();
        host.RegisterSingleton<IGraphicsDeviceService, VulkanGraphicsDeviceService>();
        host.RegisterSingleton<IResourceService, ClientResourceService>();
        host.RegisterSingleton<IRendererService, RendererService>();
        // TODO: these two services should probably resolved/registered by the WindowService being created rather than explicitly placing them here -> ask Gray about it!!!
        host.RegisterSingleton<IKeyboardService, GlfwKeyboardService>();
        host.RegisterSingleton<IMouseService, GlfwMouseService>();
        host.RegisterSingleton<CameraService, CameraService>();
        
        // Register Services Here
        host.RegisterSingleton<INetworkService, ServerNetworkService>();
        host.RegisterSingleton<IPlayerService, ServerPlayerService>();
        host.RegisterSingleton<IEntityService, ServerEntityService>();
        
        host.RegisterSingleton<EntityRenderService, EntityRenderService>();
    }
}
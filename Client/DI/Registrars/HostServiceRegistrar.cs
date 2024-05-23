using Client.Entity;
using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.Input;
using Client.Player;
using Client.Resource;
using Client.World;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Generation;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;
using Common.Resource;
using Common.World;

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
        host.RegisterSingleton<IKeyboardService, GlfwKeyboardService>();
        host.RegisterSingleton<IMouseService, GlfwMouseService>();
        
        host.RegisterSingleton<CameraService, CameraService>();
        host.RegisterSingleton<PlayerControlService, PlayerControlService>();
        
        host.RegisterSingleton<INetworkService, ServerNetworkService>();
        host.RegisterSingleton<IPlayerService, ServerPlayerService>();
        host.RegisterSingleton<IEntityService, ServerEntityService>();
        host.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        host.RegisterSingleton<IWorldService, ServerWorldService>();
        
        host.RegisterSingleton<EntityRenderService, EntityRenderService>();
        host.RegisterSingleton<WorldRenderService, WorldRenderService>();
    }
}
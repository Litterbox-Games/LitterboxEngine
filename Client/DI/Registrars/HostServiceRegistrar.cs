using Client.Entity;
using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.Input;
using Client.Graphics.Input.ImGui;
using Client.Player;
using Client.Resource;
using Client.World;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;
using Common.Resource;
using Common.World;
using Common.World.Generation;

namespace Client.DI.Registrars;

/// <summary>
///     Registers all services for a host or single player application.
/// </summary>
[RegistrarMode(EGameMode.Host | EGameMode.SinglePlayer), RegistrarPriority(EPriority.High)]
public class HostServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        container.RegisterSingleton<WindowService, WindowService>();
        container.RegisterSingleton<IGraphicsDeviceService, VulkanGraphicsDeviceService>();
        container.RegisterSingleton<ImGuiService, ImGuiService>();
        container.RegisterSingleton<IResourceService, ClientResourceService>();
        container.RegisterSingleton<RendererService, RendererService>();
        container.RegisterSingleton<InputService, InputService>();
        
        container.RegisterSingleton<CameraService, CameraService>();
        container.RegisterSingleton<PlayerControlService, PlayerControlService>();
        container.RegisterSingleton<MobControllerService, MobControllerService>();
        
        container.RegisterSingleton<INetworkService, ServerNetworkService>();
        container.RegisterSingleton<IPlayerService, ServerPlayerService>();
        container.RegisterSingleton<IEntityService, ServerEntityService>();
        container.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        container.RegisterSingleton<IWorldService, ServerWorldService>();
        
        container.RegisterSingleton<EntityRenderService, EntityRenderService>();
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
    }
}
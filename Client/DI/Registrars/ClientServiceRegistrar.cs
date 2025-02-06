using Client.Entity;
using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.Input;
using Client.Graphics.Input.ImGui;
using Client.Network;
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

namespace Client.DI.Registrars;

/// <summary>
///     Registers all client services.
/// </summary>
[RegistrarMode(EGameMode.Client), RegistrarPriority(EPriority.High)]
public class ClientServiceRegistrar : IServiceRegistrar
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
        // 
        container.RegisterSingleton<CameraService, CameraService>();
        container.RegisterSingleton<PlayerControlService, PlayerControlService>();

        container.RegisterSingleton<INetworkService, ClientNetworkService>();
        container.RegisterSingleton<IPlayerService, ClientPlayerService>();
        container.RegisterSingleton<IEntityService, ClientEntityService>();
        container.RegisterSingleton<IWorldService, ClientWorldService>();
        
        container.RegisterSingleton<EntityRenderService, EntityRenderService>();
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
    }
}
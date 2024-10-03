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
    public void RegisterServices(AbstractHost host)
    {
        host.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        host.RegisterSingleton<WindowService, WindowService>();
        host.RegisterSingleton<IGraphicsDeviceService, VulkanGraphicsDeviceService>();
        host.RegisterSingleton<ImGuiService, ImGuiService>();
        host.RegisterSingleton<IResourceService, ClientResourceService>();
        host.RegisterSingleton<RendererService, RendererService>();
        host.RegisterSingleton<InputService, InputService>();
        
        host.RegisterSingleton<CameraService, CameraService>();
        host.RegisterSingleton<PlayerControlService, PlayerControlService>();

        host.RegisterSingleton<INetworkService, ClientNetworkService>();
        host.RegisterSingleton<IPlayerService, ClientPlayerService>();
        host.RegisterSingleton<IEntityService, ClientEntityService>();
        host.RegisterSingleton<IWorldService, ClientWorldService>();
        
        host.RegisterSingleton<EntityRenderService, EntityRenderService>();
        host.RegisterSingleton<WorldRenderService, WorldRenderService>();
    }
}
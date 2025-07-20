using Autofac;
using Client.Graphics;
using Client.Graphics.Backend;
using Client.Graphics.Backend.Vulkan;
using Client.Graphics.ImGui;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Client.Services.Resource;
using Client.Services.UI;
using Common.Services.Resource;

namespace Client.Host.Registrars;

/// <summary>
///     Registers any engine services that all client hosts share.
/// </summary>
[RegistrarMode(EGameMode.Client | EGameMode.LocalHost | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Engine)]
[RegistrarPriority(EPriority.High)]
public class EngineRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(ContainerBuilder container)
    {
        container.RegisterType<WindowService>().AsSelf().SingleInstance();
        container.RegisterType<InputService>().AsSelf().SingleInstance();
        container.RegisterType<VulkanGraphicsDeviceService>().As<IGraphicsDeviceService>().AsSelf().SingleInstance();
        
        container.RegisterType<ClientResourceService>().As<IResourceService>().AsSelf().SingleInstance();
        
        container.RegisterType<RendererService>().AsSelf().SingleInstance();
        container.RegisterType<ImGuiRendererService>().AsSelf().SingleInstance();
        
        container.RegisterType<GameLoopService>().AsSelf().SingleInstance();
        container.RegisterType<MainMenuService>().AsSelf().SingleInstance();
    }
}
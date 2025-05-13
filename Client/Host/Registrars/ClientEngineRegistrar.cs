using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.GHAL.Vulkan;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Client.Services.Resource;
using Common.Services.Resource;

namespace Client.Host.Registrars;

/// <summary>
///     Registers any engine services that all client hosts share.
/// </summary>
[RegistrarMode(EGameMode.Client | EGameMode.Host | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Engine)]
[RegistrarPriority(EPriority.High)]
public class ClientEngineRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<WindowService, WindowService>();
        container.RegisterSingleton<InputService, InputService>();
        container.RegisterSingleton<IGraphicsDeviceService, VulkanGraphicsDeviceService>();
        container.RegisterSingleton<RendererService, RendererService>();
        container.RegisterSingleton<IResourceService, ClientResourceService>();
    }                                                                 
}
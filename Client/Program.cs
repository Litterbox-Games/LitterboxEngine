using System.Resources;
using Client.Graphics;
using Client.Graphics.Input;
using Client.Host;
using Common.Logging;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        using var host = new HostOrSinglePlayerHost(false);

        var logger = host.Resolve<ILoggingService>();
        ResourceManager.SetLogger(logger);

        var window = new GlfwWindowService(GraphicsBackend.Vulkan, host);
        window.Run();
    }
}
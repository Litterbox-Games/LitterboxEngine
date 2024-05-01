using System.Resources;
using Client.Graphics;
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

        var window = new Window(GraphicsBackend.Vulkan, host);
        window.Run();
    }
}
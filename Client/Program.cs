using System.Diagnostics;
using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.Input;
using Client.Host;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        using var host = new ClientHost();

        var windowService = host.Resolve<IWindowService>();
        var graphicsDeviceService = host.Resolve<IGraphicsDeviceService>();
        var rendererService = host.Resolve<IRendererService>();
        var cameraService = host.Resolve<CameraService>();
        

        windowService.OnFrame += deltaTime =>
        {
            // Update
            host.Update(deltaTime);
            
            // Draw
            rendererService.Begin(deltaTime, cameraService.Camera.ViewMatrix);
            host.Draw();
            rendererService.End();
        };
        
        windowService.Run();
        graphicsDeviceService.WaitIdle();
    }
}
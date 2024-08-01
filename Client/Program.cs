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
        using var host = new HostOrSinglePlayerHost(false);

        var windowService = host.Resolve<IWindowService>();
        var graphicsDeviceService = host.Resolve<IGraphicsDeviceService>();
        var rendererService = host.Resolve<IRendererService>();
        var cameraService = host.Resolve<CameraService>();
        
        var stopWatch = new Stopwatch();

        float deltaTime = 0;
        
        while (!windowService.ShouldClose())
        {
            stopWatch.Start();
            
            windowService.PollEvents();

            // Update
            host.Update(deltaTime);
            
            // Draw
            rendererService.Begin(cameraService.Camera.ViewMatrix);
            host.Draw();
            rendererService.End();

            stopWatch.Stop();
            deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
            stopWatch.Reset();
        }
        
        graphicsDeviceService.WaitIdle();
    }
}
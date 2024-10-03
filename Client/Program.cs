using System.Diagnostics;
using Client.Graphics;
using Client.Graphics.GHAL;
using Client.Graphics.Input;
using Client.Graphics.Input.ImGui;
using Client.Host;
using ImGuiNET;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        using var host = new ClientHost();

        var windowService = host.Resolve<WindowService>();
        var graphicsDeviceService = host.Resolve<IGraphicsDeviceService>();
        var rendererService = host.Resolve<RendererService>();
        var cameraService = host.Resolve<CameraService>();
        var imGuiService = host.Resolve<ImGuiService>();
        
        // Update
        windowService.OnUpdate += deltaTime =>
        {
            // ReSharper disable once AccessToDisposedClosure
            host.Update(deltaTime);
        };

        // Draw
        windowService.OnDraw += deltaTime =>
        {
            // Needs to be called in Draw so it happens at the same rate as imGuiService.Draw()
            imGuiService.Update(deltaTime);

            rendererService.BeginFrame();
            rendererService.BeginDrawing(cameraService.Camera.ViewMatrix);
            // ReSharper disable once AccessToDisposedClosure
            host.Draw();
            rendererService.EndDrawing();
            imGuiService.Draw();
            rendererService.EndFrame();
        };
        
        windowService.Run();
        graphicsDeviceService.WaitIdle();
    }
}
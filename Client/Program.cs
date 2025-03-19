using Client.Graphics;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.Input;
using Client.Graphics.Input.ImGui;
using Client.Host;
using Client.Resource;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        // Game Initialization
        // TODO: this will eventually be the code called when a player starts/joins a world
        using IClientHost host = new LocalHost(false);
        
        using var window = new Window();
        using var graphicsDevice = new VulkanGraphicsDevice(window);
        
        // TODO: this feels hacky, we should probably restructure the ResourceService design
        var resourceService = host.Container.Resolve<ClientResourceService>();
        resourceService.SetGraphicsDevice(graphicsDevice);
        
        // TODO: remove these as services
        var inputService = host.Container.Resolve<InputService>();
        inputService.SetWindow(window);
        
        var cameraService = host.Container.Resolve<CameraService>();
        cameraService.SetWindow(window);
        
        using var renderer = new Renderer(resourceService, graphicsDevice);
        using var imGui = new ImGuiRenderer(window, graphicsDevice);
        
        
        // Game Loop
        // TODO: turn this into a while (!window.ShouldClose()) loop instead of using lambda
        window.OnUpdate += deltaTime =>
        {
            // ReSharper disable AccessToDisposedClosure
            host.Update(deltaTime);
            
            // Needs to be called at the same rate as imGui.Draw()
            imGui.Update(deltaTime);

            renderer.BeginFrame();
            renderer.BeginDrawing(cameraService.Camera.ViewMatrix);
            
            host.Draw(renderer);
            renderer.EndDrawing();
            imGui.Draw();
            renderer.EndFrame();   
            // ReSharper enable AccessToDisposedClosure
        };
        
        window.Run();
        
        // Clean Up
        graphicsDevice.WaitIdle();
    }
}
using System.Drawing;
using System.Numerics;
using Client.Graphics;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.ImGui;
using Client.Host;
using Client.Services.Resource;
using Client.Systems;
using Common.Services.Logging;
using Silk.NET.Input;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        // Game Initialization
        // TODO: this will eventually be the code called when a player starts/joins a world
        using IClientHost host = new ClientHost();
        
        var logger = host.Container.Resolve<ILoggingService>();
        
        using var window = new Window();
        using var graphicsDevice = new VulkanGraphicsDevice(window, logger);
        
        var input = new Input(window);
        
        // TODO: this feels hacky, we should probably restructure the ResourceService design
        var resourceService = host.Container.Resolve<ClientResourceService>();
        resourceService.SetGraphicsDevice(graphicsDevice);
        
        // TODO: is there a better way to grab the camera? It would be nice if we could set the renderers camera?
        var cameraService = host.Container.Resolve<CameraSystem>();
        cameraService.SetWindow(window);
        
        using var renderer = new Renderer(resourceService, graphicsDevice);
        using var imGui = new ImGuiRenderer(window, graphicsDevice);


        var font = resourceService.Get<Font>("Fonts/dogica.otf");
        
        // Game Loop
        // TODO: turn this into a while (!window.ShouldClose()) loop instead of using lambda
        window.OnUpdate += deltaTime =>
        {
            // ReSharper disable AccessToDisposedClosure
            host.Input(input);
            
            host.Update(deltaTime);
            
            // Needs to be called at the same rate as imGui.Draw()
            imGui.Update(deltaTime);

            renderer.BeginFrame();
            renderer.BeginDrawing(cameraService.Camera.ViewMatrix);
            
            host.Draw(renderer);
            
            renderer.DrawText("Hello, World!", font, Vector2.Zero, 0.125f, 0.125f, Color.Crimson, 1);
            
            renderer.EndDrawing();
            imGui.Draw();
            renderer.EndFrame();   
            
            if (input.IsKeyDown(Key.Escape)) 
                window.SetShouldClose();
            // ReSharper enable AccessToDisposedClosure
        };
        
        window.Run();
        
        // Clean Up
        graphicsDevice.WaitIdle();
        resourceService.Dispose();
    }
}
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
    private static IClientHost MainMenu()
    {
        IClientHost? host = null;

        while (host == null)
        {
            Console.WriteLine("""
            Type letter to start associated client:
            'S' - Single-player
            'L' - Localhost
            'C' - Client
            """);
            var userInput = Console.ReadLine();

            if (string.IsNullOrEmpty(userInput)) continue;

            host = userInput.ToUpper()[0] switch
            {
                'S' => new LocalHost(true),
                'L' => new LocalHost(false),
                'C' => new ClientHost(),
                _ => host
            };
        }

        return host;
    }
    
    private static void Main()
    {
        // Engine Initialization
        var host = MainMenu();
        
        // Game Initialization
        // TODO: everything under this should be condensed to a single GameStartEvent or something similar
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
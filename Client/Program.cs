using System.Diagnostics;
using Client.Graphics;
using System.Drawing;
using System.Numerics;
using Autofac;
using Autofac.Core;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.ImGui;
using Client.Host;
using Client.Services.Resource;
using Client.Systems;
using Common.Core;
using Common.Host;
using ImGuiNET;
using Silk.NET.Input;

namespace Client;

internal static class Program
{
    private static IClientHost MainMenu(Container engineContainer)
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
            
            switch (userInput.ToUpper()[0])
            {
                case 'S':
                {
                    host = new LocalHost();
                    host.Start(engineContainer, EGameMode.SinglePlayer);
                    break;
                }
                case 'L':
                {
                    host = new LocalHost();
                    host.Start(engineContainer, EGameMode.Host);
                    break;
                }
                case 'C':
                {
                    host = new ClientHost();
                    host.Start(engineContainer, EGameMode.Client);
                    break;
                }
            }
        }
        
        return host;
    }
    
    private static void Main()
    {
        // Engine Initialization
        using var engineContainer = new Container();

        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.Host, ELifetime.Engine);
        
        var container = containerBuilder.Build();
        
        var engineUpdatables = engineContainer.RegisterUpdatables();
        
        IClientHost? host = null;
        
        // var host = MainMenu(engineContainer);
        
        // Game Initialization
        // TODO: everything under this should be condensed to a single GameStartEvent or something similar
        var window = engineContainer.Resolve<WindowService>();
        var input = engineContainer.Resolve<InputService>();
        var graphicsDevice = engineContainer.Resolve<VulkanGraphicsDeviceService>();
        var renderer = engineContainer.Resolve<RendererService>();
        var resourceService = engineContainer.Resolve<ClientResourceService>();
        
        // TODO: is there a better way to grab the camera? It would be nice if we could set the renderers camera?
        CameraSystem? cameraService = null;
        
        // TODO: convert this to use IGraphicsDevice
        using var imGui = new ImGuiRenderer(window, graphicsDevice);


        var font = resourceService.Get<Font>("Fonts/dogica.otf");
        
        // Game Loop
        var stopWatch = new Stopwatch();
        
        float deltaTime = 0;
        
        while (!window.IsClosing())
        {
            stopWatch.Start();
            window.PollEvents();
            
            engineUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
            
            host?.Input(input);
            
            host?.Update(deltaTime);
            
            // Needs to be called at the same rate as imGui.Draw()
            imGui.Update(deltaTime);

            renderer.BeginFrame();
            
            // Console.WriteLine(cameraService == null);
            
            renderer.BeginDrawing(cameraService?.Camera.ViewMatrix);

            
            // MainMenu
            if (host == null)
            {
                ImGui.Begin("Main Menu");
                
                if (ImGui.Button("Single Player"))
                {
                    host = new LocalHost();
                    host.Start(engineContainer, EGameMode.SinglePlayer);
                    cameraService = host.GameContainer?.Resolve<CameraSystem>();
                }
                
                if (ImGui.Button("Local Host"))
                {
                    host = new LocalHost();
                    host.Start(engineContainer, EGameMode.Host);
                    cameraService = host.GameContainer?.Resolve<CameraSystem>();
                }
                
                if (ImGui.Button("Client"))
                {
                    host = new ClientHost();
                    host.Start(engineContainer, EGameMode.Client);
                    cameraService = host.GameContainer?.Resolve<CameraSystem>();
                }
                
                ImGui.End();
            }
            
            
            
            host?.Draw(deltaTime, renderer);
            
            renderer.DrawText("Hello, World!", font, Vector2.Zero, 0.125f, 0.125f, Color.Crimson, 1);

            renderer.EndDrawing();
            imGui.Draw();
            renderer.EndFrame();

            if (input.IsKeyDown(Key.Escape))
                window.SetShouldClose();

            if (input.IsKeyDown(Key.X) && host != null)
            {
                graphicsDevice.WaitIdle();
                
                host.Stop();
                host.Dispose();
                host = null;
                cameraService = null;
                
                GC.Collect();
                
                Console.WriteLine("adwdadw");
            }
            
            stopWatch.Stop();
            deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
            stopWatch.Reset();
        }
        
        // Clean Up
        graphicsDevice.WaitIdle();
        resourceService.Dispose();
    }
}
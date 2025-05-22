using System.Diagnostics;
using Client.Graphics;
using Autofac;
using Autofac.Core;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.ImGui;
using Client.Host;
using Client.Services.Resource;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;
using ImGuiNET;
using Silk.NET.Input;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        // Engine Initialization

        var containerBuilder = new ContainerBuilder();
        
        containerBuilder.RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.Host, ELifetime.Engine);
        
        var engineContainer = (Container)containerBuilder.Build();

        engineContainer.Resolve<RootLoggingService>().RefreshLoggers(engineContainer);
        
        IClientHost? host = null;
        
        // Game Initialization
        // TODO: everything under this should be condensed to a single GameStartEvent or something similar
        var window = engineContainer.Resolve<WindowService>();
        var input = engineContainer.Resolve<InputService>();
        var graphicsDevice = engineContainer.Resolve<VulkanGraphicsDeviceService>();
        var renderer = engineContainer.Resolve<RendererService>();
        var resourceService = engineContainer.Resolve<ClientResourceService>();
        
        // TODO: convert this to use IGraphicsDevice
        using var imGui = new ImGuiRenderer(window, graphicsDevice);
        
        // Game Loop
        var stopWatch = new Stopwatch();
        
        float deltaTime = 0;

        var engineUpdatables = engineContainer.RegisterUpdatables();
        
        while (!window.IsClosing())
        {
            stopWatch.Start();
            window.PollEvents();

            if (host == null)
            {
                engineUpdatables.ForEach(x => x.Item2.Update(deltaTime));
            }
            
            host?.Input(input);
            host?.Update(deltaTime);
            
            // Needs to be called at the same rate as imGui.Draw()
            imGui.Update(deltaTime);

            renderer.BeginFrame();
            renderer.BeginDrawing();
            
            // MainMenu
            if (host == null)
            {
                ImGui.Begin("Main Menu");
                
                if (ImGui.Button("Single Player"))
                {
                    host = new LocalHost();
                    host.Start(engineContainer, EGameMode.SinglePlayer);
                }
                
                if (ImGui.Button("Local Host"))
                {
                    host = new LocalHost();
                    host.Start(engineContainer, EGameMode.Host);
                }
                
                if (ImGui.Button("Client"))
                {
                    host = new ClientHost();
                    host.Start(engineContainer, EGameMode.Client);
                }
                
                ImGui.End();
            }
            
            host?.Draw(deltaTime, renderer);
     
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
                
                engineContainer.Resolve<RootLoggingService>().RefreshLoggers(engineContainer);
                
                GC.Collect();
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
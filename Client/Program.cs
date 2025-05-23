using System.Diagnostics;
using Client.Graphics;
using Autofac;
using Autofac.Core;
using Client.Graphics.GHAL.Vulkan;
using Client.Graphics.ImGui;
using Client.Host;
using Client.Services.Resource;
using Client.Services.UI;
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
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.LocalHost, ELifetime.Engine);
        
        var engineContainer = (Container)containerBuilder.Build();
        
        engineContainer.Resolve<RootLoggingService>().RefreshLoggers(engineContainer);
        
        var gameLoop = engineContainer.Resolve<GameLoopService>();
        gameLoop.Run();
    }
}
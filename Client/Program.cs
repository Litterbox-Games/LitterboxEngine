using Autofac;
using Client.Services.UI;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;

namespace Client;

internal static class Program
{
    private static void Main()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.LocalHost, ELifetime.Engine);
        
        var engineContainer = containerBuilder.Build();
        
        engineContainer.Resolve<RootLoggingService>().RefreshLoggers(engineContainer);
        
        var gameLoop = engineContainer.Resolve<GameLoopService>();
        gameLoop.Run();
    }
}
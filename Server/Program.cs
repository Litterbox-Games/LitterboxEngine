using System.Diagnostics;
using System.Numerics;
using Autofac;
using Common.Core;
using Common.Core.Extensions;
using Common.Services.Entities;
using Common.Services.Logging;
using Common.Services.Network;

namespace Server;

internal static class Program
{
    private static void Main()
    {
        var container = new ContainerBuilder()
            .RegisterEngineServices()
            .RegisterGameServices(EMode.Host, true)
            .Build();
        
        var updatables = container.RegisterUpdatables();
        
        container.Resolve<RootLoggingService>().RefreshLoggers(container);
        
        var networking = container.Resolve<ServerNetworkService>();
        networking.Listen(7777);

        var mobController = container.Resolve<MobControllerService>();
        for (var x = 0; x < 30; x++)
        {
            for (var y = 0; y < 30; y++)
            {
                mobController.SpawnMobEntity(new Vector2(x * 2, y * 2));
            }
        }
        
        var stopWatch = new Stopwatch();
        float deltaTime = 0;
        
        while (true)
        {
            stopWatch.Start();
            
            updatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
            
            // 10ms to Windows causes it to wait the minimum resolution time of the clock, being around 15ms.
            // Values above 10 will cause instability in the timing.
            // On a Linux system, this will roughly be 10ms. fuck you Microsoft
            Thread.Sleep(10);
            
            stopWatch.Stop();
            deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
            stopWatch.Reset();
        }
    }
}
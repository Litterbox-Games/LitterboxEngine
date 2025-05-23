using System.Diagnostics;
using Autofac;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Server.Host;

namespace Server;

internal static class Program
{
    private static void Main()
    {
        var builder = new ContainerBuilder();
        
        builder.RegisterServices(EGameMode.Dedicated, ELifetime.Engine);
        
        var engineContainer = builder.Build();
        
        var engineUpdatables = engineContainer.RegisterUpdatables();
        
        using IServerHost host = new ServerHost();
        host.Start(engineContainer);

        var stopWatch = new Stopwatch();

        float deltaTime = 0;
        
        while (true)
        {
            stopWatch.Start();
            
            engineUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
            host.Update(deltaTime);
            
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
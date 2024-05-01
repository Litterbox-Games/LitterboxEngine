using System.Diagnostics;
using Common.Logging;
using Common.Resource;
using Server.Host;

namespace Server;

internal static class Program
{
    private static void Main()
    {
        using var host = new ServerHost();

        var logger = host.Resolve<ILoggingService>();
        ResourceManager.SetLogger(logger);
        
        var stopWatch = new Stopwatch();

        float deltaTime = 0;
        
        while (true)
        {
            stopWatch.Start();
            
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
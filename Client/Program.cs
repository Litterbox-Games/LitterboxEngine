using Autofac;
using Client.Services.UI;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;

namespace Client;

internal static class Program
{
    private static void Main() => 
        new ContainerBuilder()
            .RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.LocalHost, ELifetime.Engine)
            .Build()
            .Resolve<GameLoopService>()
            .Run();
}
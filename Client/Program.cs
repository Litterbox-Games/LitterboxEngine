using Autofac;
using Client.Services.UI;
using Common.Core.Extensions;

namespace Client;

internal static class Program
{
    private static void Main() => 
        new ContainerBuilder()
            .RegisterEngineServices()
            .Build()
            .Resolve<GameLoopService>()
            .Run();
}
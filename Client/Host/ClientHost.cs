using Client.Network;
using Common.Host;
using Common.Player;

namespace Client.Host;

/// <summary>
///     The host used to represent the client game state.
/// </summary>
public class ClientHost : BaseClientHost
{
    public ClientHost(): base(EGameMode.Client)
    {
        // Warm Service Singletons
        Container.Resolve<IPlayerService>();
        
        var networkService = Container.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
    }
}
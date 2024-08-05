using Client.Network;
using Common.Host;
using Common.Network;
using Common.Player;

namespace Client.Host;

/// <summary>
///     the host used to represent the client game state.
/// </summary>
public class ClientHost : AbstractHost
{
    /// <inheritdoc />
    public ClientHost() : base(EGameMode.Client)
    {
        RegisterServices();

        var networkService = Resolve<ClientNetworkService>();
        
        // Warm Service Singletons
        Resolve<IPlayerService>();
        
        networkService.Connect("127.0.0.1", 7777);
    }
}
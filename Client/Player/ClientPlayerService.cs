using Client.Network;
using Common.DI;
using Common.Network;
using Common.Player;
using Common.Player.Messages;

namespace Client.Player;

public class ClientPlayerService : IPlayerService
{
    public ulong PlayerId { get; }

    public IEnumerable<NetworkPlayer> Players => _players;

    private readonly List<NetworkPlayer> _players = [];
    
    public ClientPlayerService(IClientNetworkService networkService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        networkService.RegisterMessageHandle<PlayerConnectMessage>(OnPlayerConnectMessage);
        networkService.RegisterMessageHandle<PlayerDisconnectMessage>(OnPlayerDisconnectMessage);
        networkService.RegisterMessageHandle<PlayerListSyncMessage>(OnPlayerListSyncMessage);
    }

    private void OnPlayerConnectMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (PlayerConnectMessage) message;
        _players.Add(castedMessage.NetworkPlayer!);
    }

    private void OnPlayerDisconnectMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (PlayerDisconnectMessage) message;

        _players.Remove(_players.First(x => x.PlayerID == castedMessage.PlayerId));
    }

    private void OnPlayerListSyncMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (PlayerListSyncMessage) message;

        _players.Clear();
        _players.AddRange(castedMessage.Players);
    }
}
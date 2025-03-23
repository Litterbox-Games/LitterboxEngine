using Client.Services.Network;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Players.Messages;

namespace Client.Services.Players;

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

    private void OnPlayerConnectMessage(PlayerConnectMessage message, NetworkPlayer? _)
    {
        _players.Add(message.NetworkPlayer!);
    }

    private void OnPlayerDisconnectMessage(PlayerDisconnectMessage message, NetworkPlayer? _)
    {
        _players.Remove(_players.First(x => x.PlayerId == message.PlayerId));
    }

    private void OnPlayerListSyncMessage(PlayerListSyncMessage message, NetworkPlayer? _)
    {
        _players.Clear();
        _players.AddRange(message.Players);
    }
}
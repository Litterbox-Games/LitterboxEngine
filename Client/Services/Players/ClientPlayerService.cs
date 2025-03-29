using Client.Services.Network;
using Common.Services.Events;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Players.Messages;

namespace Client.Services.Players;

public class ClientPlayerService : IPlayerService
{
    public ulong PlayerId { get; }

    public IEnumerable<NetworkPlayer> Players => _players;

    private readonly List<NetworkPlayer> _players = [];
    
    public ClientPlayerService(EventService eventService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        eventService.Handle<PlayerConnectMessage>(OnPlayerConnectMessage);
        eventService.Handle<PlayerDisconnectMessage>(OnPlayerDisconnectMessage);
        eventService.Handle<PlayerListSyncMessage>(OnPlayerListSyncMessage);
    }

    private void OnPlayerConnectMessage(PlayerConnectMessage message)
    {
        _players.Add(message.NetworkPlayer!);
    }

    private void OnPlayerDisconnectMessage(PlayerDisconnectMessage message)
    {
        _players.Remove(_players.First(x => x.PlayerId == message.PlayerId));
    }

    private void OnPlayerListSyncMessage(PlayerListSyncMessage message)
    {
        _players.Clear();
        _players.AddRange(message.Players);
    }
}
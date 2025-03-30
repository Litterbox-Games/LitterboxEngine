using Common.Core;
using Common.Host;
using Common.Services.Events;
using Common.Services.Network;
using Common.Services.Players.Events;

namespace Common.Services.Players;

public sealed class ServerPlayerService : IPlayerService
{
    public ulong PlayerId { get; }
    public IEnumerable<NetworkPlayer> Players => _network.Players;

    private readonly ServerNetworkService _network;
    private readonly EventService _eventService;

    public ServerPlayerService(ServerNetworkService networkService, EventService eventService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        _network = networkService;
        _eventService = eventService;
        
        eventService.Handle<PlayerConnectEvent>(OnPlayerConnect);
    }

    private void OnPlayerConnect(PlayerConnectEvent e)
    {
        var syncMessage = new PlayerListSyncEvent();

        foreach (var p in Players)
        {
            syncMessage.Players.Add(p);
        }

        syncMessage.Receivers = serverPlayer => serverPlayer == e.NetworkPlayer; 
        _eventService.Emit(syncMessage);
    }
}
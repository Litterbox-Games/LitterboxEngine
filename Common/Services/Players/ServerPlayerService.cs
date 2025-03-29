using Common.Core;
using Common.Host;
using Common.Services.Events;
using Common.Services.Network;
using Common.Services.Players.Messages;

namespace Common.Services.Players;

public sealed class ServerPlayerService : IPlayerService
{
    public ulong PlayerId { get; }
    public IEnumerable<NetworkPlayer> Players => _network.Players;

    private readonly ServerNetworkService _network;
    private readonly IContainer _container;
    private readonly EventService _eventService;

    public ServerPlayerService(IContainer container, ServerNetworkService networkService, EventService eventService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        _container = container;
        _network = networkService;
        _eventService = eventService;
        
        eventService.Handle<PlayerConnectMessage>(OnPlayerConnect);
        eventService.Handle<PlayerDisconnectMessage>(OnPlayerDisconnect);
    }

    private void OnPlayerConnect(PlayerConnectMessage e)
    {
        var syncMessage = new PlayerListSyncMessage();

        foreach (var p in Players)
        {
            syncMessage.Players.Add(p);
        }

        syncMessage.Receivers = serverPlayer => serverPlayer == e.Sender; 
        _eventService.Emit(syncMessage);

        // if (Players.Count() < 2)
        // {
        //     return;
        // }

        // var connectMessage = new PlayerConnectMessage
        // {
        //     NetworkPlayer = e.Sender,
        //     Receivers = serverPlayer => serverPlayer != e.Sender
        // };
        // 
        // _eventService.Emit(connectMessage);
    }

    private void OnPlayerDisconnect(PlayerDisconnectMessage e)
    {
        if (!Players.Any() || Players.Count() == 2 && _container.GameMode == EGameMode.Host)
            return;

        var disconnectMessage = new PlayerDisconnectMessage
        {
            PlayerId = e.PlayerId,
            Receivers = serverPlayer => serverPlayer != e.Sender
        };

        _eventService.Emit(disconnectMessage);
    }
}
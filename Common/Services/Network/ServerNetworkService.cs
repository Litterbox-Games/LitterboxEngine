using Common.Core;
using Common.Host;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network.Events;
using Common.Services.Players;
using Common.Services.Players.Events;
using Lidgren.Network;

namespace Common.Services.Network;

public sealed class ServerNetworkService(IContainer container, ILoggingService logger, EventService eventService): NetworkService(logger, eventService)
{
    private NetServer? _server;
    protected override NetPeer NetPeer => _server!;

    private readonly ILoggingService _logger = logger;
    private readonly EventService _eventService = eventService;
    private IPlayerService? _playerService;
    
    private readonly List<ServerPlayer> _players = [];
    public IEnumerable<ServerPlayer> Players => _players;
    
    protected override void OnOutgoing(OutgoingEvent e)
    {
        IEnumerable<ServerPlayer> players = _players;
        if (e.NetworkEvent.Receivers != null)
            players = players.Where(p => e.NetworkEvent.Receivers(p));
        
        var connections = players.Select(x => x.PlayerConnection).Where(x => x != null).Cast<NetConnection>().ToList();

        if (connections.Count == 0) return;
        
        SendMessage(connections, e.NetworkEvent);
    }
    
    public override void Update(float deltaTime)
    {
        if (_server == null)
            return;
        
        if (_server.Status != NetPeerStatus.Running)
            return;
        
        while (_server.ReadMessage() is { } message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus) message.ReadByte();
                    var reason = message.ReadString();

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            OnConnect(message.SenderConnection);
                            continue;
                        case NetConnectionStatus.Disconnected:
                            OnDisconnect(message.SenderConnection);
                            continue;
                    }

                    OnStatusChange(status, reason);

                    break;
                case NetIncomingMessageType.ConnectionApproval:
                    var approve = OnConnectionRequest(message);

                    if (approve)
                        message.SenderConnection.Approve();
                    else
                        message.SenderConnection.Deny();

                    break;

                case NetIncomingMessageType.Data:
                    var e = OnData(message);
                    if (e == null) break;
                    e.Sender = _players.FirstOrDefault(x => x.PlayerConnection == message.SenderConnection);
                    _eventService.Incoming(e);
                    break;
            }
        }
    }

    public void Listen(ushort port)
    {
        if (_server != null) throw new InvalidOperationException("Server is already listening and must be destroyed.");
        
        var config = new NetPeerConfiguration("Ages of Automation") { 
            Port = port,
            PingInterval = 1f,
            ConnectionTimeout = 5f
        };

        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        
        _server = new NetServer(config);
        _server.Start();

        _playerService = container.Resolve<IPlayerService>();
        
        _logger.Information("Server is now listening on port 7777.");
        
        if (container.GameMode == EGameMode.Host || container.GameMode == EGameMode.SinglePlayer)
        {
            _players.Add(new ServerPlayer(_playerService.PlayerId, $"Player {_playerService.PlayerId}", null));
        }
        
        _eventService.Emit(new StartEvent());
    }

    public void StopListening()
    {
        if (_server == null) return;
        
        _eventService.Emit(new StopEvent());

        _server.Shutdown("Server has been shutdown by host.");

        _server = null;
    }
    
    private bool OnConnectionRequest(NetIncomingMessage message)
    {
        if (container.GameMode == EGameMode.SinglePlayer)
            return false;
        
        try
        {
            var player = new ServerPlayer(message.ReadUInt64(), message.ReadString(), message.SenderConnection);
            _players.Add(player);
        }
        catch (Exception e)
        {
            _logger.Error("Encountered an error during a connection request!");
            _logger.Error(e.Message);
            if (e.StackTrace != null) _logger.Error(e.StackTrace);
            
            return false;
        }

        return true;
    }

    private void OnConnect(NetConnection conn)
    {
        var player =
            _players.FirstOrDefault(x => x.PlayerConnection?.RemoteUniqueIdentifier == conn.RemoteUniqueIdentifier);

        if (player == null)
        {
            conn.Disconnect("Authentication failed or wasn't performed.");

            return;
        }

        // EventOnPlayerConnect?.Invoke(player);
        _eventService.Emit(new PlayerConnectEvent { NetworkPlayer = player, Receivers = serverPlayer => serverPlayer != player });

        _logger.Information($"{player.PlayerName} has connected!");

        // Synchronize client and server state
    }

    private void OnDisconnect(NetConnection conn)
    {
        var player = _players.FirstOrDefault(x => x.PlayerConnection?.RemoteUniqueIdentifier == conn.RemoteUniqueIdentifier);

        if (player == null)
            return;

        _players.Remove(player);

        _logger.Information($"{player.PlayerName} has disconnected!");
        
        var disconnectMessage = new PlayerDisconnectEvent
        {
            PlayerId = player.PlayerId,
            Receivers = serverPlayer => serverPlayer != player
        };

        _eventService.Emit(disconnectMessage);
    }
    
    private void OnStatusChange(NetConnectionStatus newStatus, string reason)
    {
        _logger.Debug($"Server status changed to {newStatus} for reason: {reason}!");
    }
}
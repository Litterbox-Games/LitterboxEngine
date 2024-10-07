using Common.DI;
using Common.Host;
using Common.Logging;
using Common.Player;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Network;

public class ServerNetworkService : AbstractNetworkService
{
    private NetServer? _server;
    protected override NetPeer? NetPeer => _server;
    
    public event Action? EventOnStartListen;
    public event Action? EventOnStopListen;
    public event Action? EventOnPreStopListen;
    public event Action<ServerPlayer>? EventOnPlayerConnect;
    public event Action<ServerPlayer>? EventOnPlayerDisconnect;

    private IPlayerService? _playerService;
    public IEnumerable<ServerPlayer> Players => _players;
    
    private readonly List<ServerPlayer> _players = new();

    public ServerNetworkService(IHost host, ILoggingService logger) : base(host, logger) { }

    public override void Update(float deltaTime)
    {
        if (_server == null)
            return;
        
        if (_server.Status != NetPeerStatus.Running)
            return;
        
        while (_server.ReadMessage() is { } incomingMsg)
        {
            switch (incomingMsg.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus) incomingMsg.ReadByte();
                    var reason = incomingMsg.ReadString();

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            OnConnect(incomingMsg.SenderConnection);
                            continue;
                        case NetConnectionStatus.Disconnected:
                            OnDisconnect(incomingMsg.SenderConnection);
                            continue;
                    }

                    OnStatusChange(status, reason);

                    break;
                case NetIncomingMessageType.ConnectionApproval:
                    var approve = OnConnectionRequest(incomingMsg);

                    if (approve)
                        incomingMsg.SenderConnection.Approve();
                    else
                        incomingMsg.SenderConnection.Deny();

                    break;

                case NetIncomingMessageType.Data:
                    OnData(incomingMsg);
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

        _playerService = Host.Resolve<IPlayerService>();
        
        Logger.Information("Server is now listening on port 7777.");
        
        if (Host.GameMode == EGameMode.Host || Host.GameMode == EGameMode.SinglePlayer)
        {
            PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
            _players.Add(new ServerPlayer(PlayerId, $"Player {PlayerId}", null));
        }
        
        EventOnStartListen?.Invoke();
    }

    public void StopListening()
    {
        if (_server == null) return;

        EventOnPreStopListen?.Invoke();

        _server.Shutdown("Server has been shutdown by host.");

        EventOnStopListen?.Invoke();

        _server = null;
    }

    public void SendToPlayer(INetworkMessage message, ServerPlayer player)
    {
        if (player.PlayerConnection == null)
            throw new ArgumentNullException(nameof(player), "The player object passed must have a valid connection to receive a packet.");
        
        SendMessage(player.PlayerConnection, message);
    }

    public void SendToAllPlayers(INetworkMessage message)
    {
        if (_playerService == null)
            throw new InvalidOperationException("Cannot send message to clients without a running server");

        var connections = _playerService.Players.Cast<ServerPlayer>().Select(x => x.PlayerConnection).Where(x => x != null).Cast<NetConnection>();

        if (!connections.Any())
            return;
        
        SendMessage(_playerService.Players.Cast<ServerPlayer>().Select(x => x.PlayerConnection).Where(x => x != null).Cast<NetConnection>(), message);
    }
    
    private void OnData(NetIncomingMessage message)
    {
        var player = _players.FirstOrDefault(x => x.PlayerConnection == message.SenderConnection);

        if (player == null)
        {
            Logger.Warning("An unknown connection attempted to send a packet.");
            return;
        }

        var messageId = message.ReadInt32();

        if (!Messages.ContainsKey(messageId))
        {
            Logger.Warning(
                $"A player ${player.PlayerID} attempted to send an invalid message with the ID ${messageId}.");
            return;
        }

        var messageType = Messages[messageId];

        var castedMessage = (INetworkMessage) Activator.CreateInstance(messageType)!;

        castedMessage.Deserialize(message);

        if (!MessageHandles.ContainsKey(messageType))
        {
            Logger.Warning(
                $"A player ${player.PlayerID} attempted to send an message with the ID ${messageId} that has no valid handles.");
            return;
        }

        MessageHandles[messageType].ForEach(x => x.Invoke(castedMessage, player));
    }
    
    private bool OnConnectionRequest(NetIncomingMessage message)
    {
        if (Host.GameMode == EGameMode.SinglePlayer)
            return false;
        
        try
        {
            var player = new ServerPlayer(message.ReadUInt64(), message.ReadString(), message.SenderConnection);
            _players.Add(player);
        }
        catch (Exception e)
        {
            Logger.Error("Encountered an error during a connection request!");
            Logger.Error(e.Message);
            Logger.Error(e.StackTrace);

            return false;
        }

        return true;
    }

    private void OnConnect(NetConnection conn)
    {
        var player =
            _players.FirstOrDefault(x => x.PlayerConnection?.RemoteUniqueIdentifier == conn!.RemoteUniqueIdentifier);

        if (player == null)
        {
            conn!.Disconnect("Authentication failed or wasn't performed.");

            return;
        }

        EventOnPlayerConnect?.Invoke(player);

        Logger.Information($"{player.PlayerName} has connected!");

        // Synchronize client and server state
    }

    private void OnDisconnect(NetConnection conn)
    {
        var player =
            _players.FirstOrDefault(x => x.PlayerConnection?.RemoteUniqueIdentifier == conn!.RemoteUniqueIdentifier);

        if (player == null)
            return;

        EventOnPlayerDisconnect?.Invoke(player);

        _players.Remove(player);

        Logger.Information($"{player.PlayerName} has disconnected!");
    }
    
    private void OnStatusChange(NetConnectionStatus newStatus, string reason)
    {
        Logger.Debug("Server status changed to {newStatus} for reason: {reason}!");
    }
}
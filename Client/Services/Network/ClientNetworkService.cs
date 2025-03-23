using Common.DI;
using Common.Logging;
using Common.Network;
using Common.Players;
using Lidgren.Network;

namespace Client.Network;

public class ClientNetworkService : IClientNetworkService
{
    public Dictionary<int, Type> Messages { get; } = [];
    public Dictionary<Type, List<OnMessage>> MessageHandles { get; } = [];
    public NetPeer NetPeer => _client;
    
    public event Action? EventOnConnect;
    public event Action? EventOnDisconnect;
    
    private readonly NetClient _client;
    private readonly IContainer _container;
    private readonly ILoggingService _logger;
    
    private NetConnection? _connection;
    private float _connectionAttemptTime;
    
    public ClientNetworkService(IContainer container, ILoggingService logger)
    {
        _container = container;
        _logger = logger;
        (this as INetworkService).RegisterMessageTypes(_logger);
        
        var config = new NetPeerConfiguration("Ages of Automation") { 
            PingInterval = 1f,
            ConnectionTimeout = 5f
        };
        
        _client = new NetClient(config);
        _client.Start();
    }
    
    public void Connect(string ip, ushort port)
    {
        // Create a random ID and send it in the approval request message
        var msg = _client.CreateMessage();

        var playerService = _container.Resolve<IPlayerService>();
        
        var playerName = $"Player {playerService.PlayerId}";

        msg.Write(playerService.PlayerId);
        msg.Write(playerName);

        _connection = _client.Connect(ip, port, msg);
    }

    public void Disconnect()
    {
        _client.Disconnect("Client has left the server.");
        _client.FlushSendQueue();
        Thread.Sleep(100);
    }

    public void Update(float deltaTime)
    {
        while (_client.ReadMessage() is { } incomingMsg)
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
                            EventOnConnect?.Invoke();
                            continue;
                        case NetConnectionStatus.Disconnected:
                            EventOnDisconnect?.Invoke();
                            _connection = null;
                            continue;
                    }

                    OnStatusChange(status, reason);

                    break;
                case NetIncomingMessageType.Data:
                    OnData(incomingMsg); 
                    break; 
            }
        }

        if (_client.ConnectionStatus != NetConnectionStatus.Disconnected) return;

        if (_connectionAttemptTime < 5f)
        {
            _connectionAttemptTime += deltaTime;
            return;
        }

        Disconnect();
    }

    public void SendToServer(INetworkMessage message)
    {
        (this as INetworkService).SendMessage(_connection!, message);
    }

    private void OnData(NetIncomingMessage message)
    {
        var messageId = message.ReadInt32();

        if (!Messages.TryGetValue(messageId, out var messageType))
        {
            _logger.Error($"The server attempted to send an invalid message with the ID ${messageId}.");
            return;
        }

        if (!MessageHandles.TryGetValue(messageType, out var handlers))
        {
            _logger.Error(
                $"The server attempted to send an message with the ID ${messageId} that has no valid handles.");
            return;
        }
        
        var castedMessage = (INetworkMessage) Activator.CreateInstance(messageType)!;

        castedMessage.Deserialize(message);
        handlers.ForEach(x => x.Invoke(castedMessage, null));
    }
    
    private void OnStatusChange(NetConnectionStatus newStatus, string reason)
    {
        _logger.Information($"Client status changed to {newStatus} for the reason: {reason}");
    }
}
using Common.Core;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network.Events;
using Lidgren.Network;

namespace Common.Services.Network;

public abstract class NetworkService: IService, IUpdatable
{
    protected abstract NetPeer NetPeer { get; }

    private readonly Dictionary<int, Type> _events = new();

    private readonly ILoggingService _logger;
    private readonly EventService _eventService;
    
    protected NetworkService(ILoggingService logger, EventService eventService)
    {
        _logger = logger;
        _eventService = eventService;

        eventService.Handle<OutgoingEvent>(OnOutgoing);
        eventService.Handle<RegisterMessageEvent>(OnRegisterMessage);
    }
    
    protected abstract void OnOutgoing(OutgoingEvent e);
    
    private void OnRegisterMessage(RegisterMessageEvent e) 
    {
        var hash = GetDeterministicHashCode(e.Type.FullName!);

        if (_events.TryGetValue(hash, out var message))
        {
            _logger.Warning("Attempted to register network events sharing the same hash.");
            _logger.Warning(message.FullName!);
            _logger.Warning(e.Type.FullName!);

            return;
        }

        _events[hash] = e.Type;
    }
    
    protected void SendMessage(NetConnection connection, INetworkEvent e)
    {
        var messageId = GetDeterministicHashCode(e.GetType().FullName!);

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        e.Serialize(packet);

        NetPeer.SendMessage(packet, connection, e.NetworkChannel);
    }

    protected void SendMessage(IEnumerable<NetConnection> connections, INetworkEvent e)
    {
        var messageId = GetDeterministicHashCode(e.GetType().FullName!);

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        e.Serialize(packet);
        
        NetPeer.SendMessage(packet, connections.ToList(), e.NetworkChannel, 0);
    }
    
    
    protected INetworkEvent? OnData(NetIncomingMessage message)
    {
        var messageId = message.ReadInt32();

        if (!_events.TryGetValue(messageId, out var messageType))
        {
            _logger.Error($"Received an invalid message with the ID {messageId}.");
            return null;
        }
        
        var castedMessage = (INetworkEvent) Activator.CreateInstance(messageType)!;
        castedMessage.Deserialize(message);
        
        return castedMessage;
    }
    
    private static int GetDeterministicHashCode(string str)
    {
        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;
    
            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }
    
            return hash1 + (hash2 * 1566083941);
        }
    }

    public abstract void Update(float deltaTime);

    public virtual void Dispose()
    {
        _eventService.Unhandle<OutgoingEvent>(OnOutgoing);
        _eventService.Unhandle<RegisterMessageEvent>(OnRegisterMessage);
    }
}
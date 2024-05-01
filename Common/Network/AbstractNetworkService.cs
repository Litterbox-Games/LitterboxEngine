using Common.DI;
using Common.Entity.Messages;
using Common.Logging;
using Common.Player.Messages;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Network;

public abstract class AbstractNetworkService : INetworkService
{
    protected readonly Dictionary<int, Type> Messages = new();
    protected readonly Dictionary<Type, List<OnMessage>> MessageHandles = new();
    protected abstract NetPeer? NetPeer { get; }

    protected IHost Host;
    protected ILoggingService Logger;

    public ulong PlayerId { get; protected set; } = 0;
    
    public AbstractNetworkService(IHost host, ILoggingService logger)
    {
        Host = host;
        Logger = logger;
        
        RegisterMessageType<PlayerConnectMessage>();
        RegisterMessageType<PlayerDisconnectMessage>();
        RegisterMessageType<PlayerListSyncMessage>();
        
        RegisterMessageType<EntitySpawnMessage>();
        RegisterMessageType<EntityDespawnMessage>();
        RegisterMessageType<EntityMoveMessage>();
    }

    public virtual void Update(float deltaTime) { }
    public virtual void Draw() { }

    public void SendMessage(NetConnection connection, INetworkMessage message)
    {
        if (NetPeer == null) throw new InvalidOperationException("Cannot send a packet without first initializing the network.");
        
        var messageId = Messages.Where(x => x.Value == message.GetType()).Select(x => x.Key).First();

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        message.Serialize(packet);

        NetPeer.SendMessage(packet, connection, message.NetworkChannel);
    }

    public void SendMessage(IEnumerable<NetConnection> connections, INetworkMessage message)
    {
        if (NetPeer == null) throw new InvalidOperationException("Cannot send a packet without first initializing the network.");
        
        var messageId = Messages.Where(x => x.Value == message.GetType()).Select(x => x.Key).First();

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        message.Serialize(packet);
        
        //connections.ForEach(x => NetPeer.SendMessage(packet, x, message.NetworkChannel));

        NetPeer.SendMessage(packet, connections.ToList(), message.NetworkChannel, 0);
    }

    public void RegisterMessageType<T>() where T : INetworkMessage, new()
    {
        var hash = GetDeterministicHashCode(typeof(T).FullName!);

        if (Messages.TryGetValue(hash, out var message))
        {
            Logger.Warning("Attempted to register messages sharing the same hash.");
            Logger.Warning(message.FullName!);
            Logger.Warning(typeof(T).FullName!);

            return;
        }

        Messages[hash] = typeof(T);
    }

    public void RegisterMessageHandle<T>(OnMessage handle) where T : INetworkMessage, new()
    {
        if (MessageHandles.ContainsKey(typeof(T)))
            MessageHandles[typeof(T)].Add(handle);
        else
            MessageHandles[typeof(T)] = new List<OnMessage> {handle};
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
}

public delegate void OnMessage(INetworkMessage message, Player.Player? player);
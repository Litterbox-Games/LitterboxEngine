using Common.Core;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.Events;

public interface IEvent;

public interface INetworkEvent : IEvent
{
    public NetDeliveryMethod NetworkChannel { get; }
    public ServerPlayer? Sender { get; set; }
    public Predicate<ServerPlayer>? Receivers { get; set; }

    public void Serialize(NetOutgoingMessage writer);
    public void Deserialize(NetIncomingMessage reader);
}

public delegate void OnEvent<in T>(T e) where T : IEvent;

public class EventService(ILoggingService logger) : IService
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    // Set by NetworkService
    public NetworkService Network =  null!;

    public void EmitIncoming(INetworkEvent e)
    {
        var eventType = e.GetType();
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            logger.Warning($"Attempted to emit an event ${eventType.FullName} that has no valid handlers.");
            return;
        }
        
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var handler in handlers)
        {
            var handlerType = handler.GetType();
            var delegateType = typeof(OnEvent<>).MakeGenericType(eventType);

            if (!handlerType.IsAssignableFrom(delegateType)) continue;
            
            handler.DynamicInvoke(e);
        }
    }
    
    public void EmitOutgoing(INetworkEvent e)
    {
        Network.Send(e);
    }
    
    public void Emit(IEvent e)
    {
        if (e is INetworkEvent netEvent)
        {
            Network.Send(netEvent);
        }
        
        var eventType = e.GetType();
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            if (e is not INetworkEvent)
                logger.Warning($"Attempted to emit an event ${eventType.FullName} that has no valid handlers.");
            return;
        }
        
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var handler in handlers)
        {
            var handlerType = handler.GetType();
            var delegateType = typeof(OnEvent<>).MakeGenericType(eventType);

            if (!handlerType.IsAssignableFrom(delegateType)) continue;
            
            handler.DynamicInvoke(e);
        }
    } 
    
    public void Handle<T>(OnEvent<T> handler) where T : IEvent, new()
    {
        if (_handlers.ContainsKey(typeof(T)))
        {
            _handlers[typeof(T)].Add(handler);
        }
        else
        {
            if (typeof(T).IsAssignableTo(typeof(INetworkEvent)))
                Network.RegisterMessageType(typeof(T));
            
            _handlers[typeof(T)] = [handler];
        }
            
    }
}
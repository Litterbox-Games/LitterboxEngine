using Common.Core;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Network.Events;

namespace Common.Services.Events;

public delegate void OnEvent<in T>(T e) where T : IEvent;

public class EventService(ILoggingService logger) : IService
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Incoming(INetworkEvent e) => CallHandlers(e);
    
    public void Outgoing(INetworkEvent e) => CallHandlers(new OutgoingEvent(e));
    
    
    public void Emit(IEvent e)
    {
        if (e is INetworkEvent networkEvent)
            Outgoing(networkEvent);
        
        CallHandlers(e);
    }

    private void CallHandlers(IEvent e)
    {
        var eventType = e.GetType();
        
        if (!_handlers.TryGetValue(eventType, out var handlers)) return;
        
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
                CallHandlers(new RegisterMessageEvent(typeof(T)));
            
            _handlers[typeof(T)] = [handler];
        }
            
    }
}
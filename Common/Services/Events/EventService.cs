using Common.Core;
using Common.Services.Logging;

namespace Common.Services.Events;

public interface IEvent;

public struct TestEvent : IEvent
{
    public string Foo;
}

public delegate void OnEvent<in T>(T e) where T : IEvent;

public class EventService: IService
{
    private readonly Dictionary<int, Type> _events = new();
    
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    private readonly ILoggingService _logger;
    
    public EventService(ILoggingService logger)
    {
        _logger = logger;
        RegisterEvents();
    }
    
    public void Emit(IEvent e)
    {
        // TODO: will this work?
        var hash = GetDeterministicHashCode(e.GetType().FullName!);
        
        // TODO: log name instead of hash?
        if (!_events.TryGetValue(hash, out var eventType))
        {
            _logger.Warning($"Attempted to emit an invalid event with the hash ${hash}.");
            return;
        }
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.Warning($"Attempted to emit an event with the hash ${hash} that has no valid handles.");
            return;
        }
        
        foreach (var handler in handlers)
        {
            var handlerType = handler.GetType();
            var delegateType = typeof(OnEvent<>).MakeGenericType(eventType);

            if (!handlerType.IsAssignableFrom(delegateType))return;
            
            handler.DynamicInvoke(e);
        }
    } 
    
    private void RegisterEvents()
    {
        RegisterEvent<TestEvent>();
    }
    
    private void RegisterEvent<T>() where T : IEvent, new()
    {
        var hash = GetDeterministicHashCode(typeof(T).FullName!);

        _logger.Information($"Registering message '{typeof(T).Name}'");
        if (_events.TryGetValue(hash, out var message))
        {
            _logger.Warning("Attempted to register messages sharing the same hash.");
            _logger.Warning(message.FullName!);
            _logger.Warning(typeof(T).FullName!);

            return;
        }

        _events[hash] = typeof(T);
    }
    
    public void Handle<T>(OnEvent<T> handler) where T : IEvent, new()
    {
        if (_handlers.ContainsKey(typeof(T)))
            _handlers[typeof(T)].Add(handler);
        else
            _handlers[typeof(T)] = [handler];
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
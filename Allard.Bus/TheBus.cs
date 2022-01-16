using System.Collections.Concurrent;
using System.Reflection;

namespace Allard.Bus;

public class TheBus : ISubscriber, IPublisher
{
    private record EventHandlerMethod(MethodInfo method, object Target)
    {
        public void Invoke(object value) => method.Invoke(Target, new[] {value});
        
    }
    private readonly ConcurrentDictionary<Type, List<EventHandlerMethod>> _subscribers = new();
        
    public Task Subscribe<T>(Action<T> execute)
    {
        var handlers = _subscribers.GetOrAdd(typeof(T), t => new List<EventHandlerMethod>());
        handlers.Add(new EventHandlerMethod(execute.Method, execute.Target));
        return Task.CompletedTask;
    }

    public Task Publish<T>(T message)
    {
        // todo: async
        if (!_subscribers.TryGetValue(typeof(T), out var subscribersForObject)) return Task.CompletedTask;
        foreach (var s in subscribersForObject)
        {
            s.Invoke(message);
        }

        return Task.CompletedTask;
    }
}
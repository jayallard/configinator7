namespace Allard.Configinator.Core;

public interface IEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task ExecuteAsync(TEvent evt, CancellationToken token = default);
}
namespace Allard.DomainDrivenDesign;

public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task ExecuteAsync(TEvent evt, CancellationToken cancellationToken = default);
}
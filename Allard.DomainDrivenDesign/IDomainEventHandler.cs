namespace Allard.DomainDrivenDesign;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task ExecuteAsync(TEvent evt, CancellationToken cancellationToken = default);
}
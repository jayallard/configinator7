namespace Allard.DomainDrivenDesign;

public interface IEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
namespace Allard.Configinator.Core;

public interface IEventPublisher
{
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
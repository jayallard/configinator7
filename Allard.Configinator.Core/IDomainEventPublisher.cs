namespace Allard.Configinator.Core;

public interface IDomainEventPublisher
{
    Task PublishAsync(IEnumerable<ISourceEvent> events, CancellationToken cancellationToken = default);
}
namespace Allard.Configinator.Core;

public interface IEventPublisher
{
    Task PublishAsync(IEnumerable<ISourceEvent> events, CancellationToken cancellationToken = default);
}
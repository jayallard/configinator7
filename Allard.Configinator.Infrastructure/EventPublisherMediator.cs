using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Infrastructure;

public class EventPublisherMediator : IEventPublisher
{
    private readonly Mediator _mediator;

    public EventPublisherMediator(Mediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync(IEnumerable<ISourceEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var e in events)
        {
            await _mediator.Publish(e, cancellationToken);
        }
    }
}
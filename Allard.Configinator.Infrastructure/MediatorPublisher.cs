using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;
using MediatR;

namespace Allard.Configinator.Infrastructure;

public class MediatorPublisher : IEventPublisher
{
    private readonly IMediator _mediator;

    public MediatorPublisher(IMediator mediator)
    {
        _mediator = Guards.HasValue(mediator, nameof(mediator));
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var evt in events)
        {
            // todo: how to handle this better.. maybe expressions?
            // MediatorNotification must be of type T in order
            // for the Mediator to match the notification
            // handlers.
            var type = typeof(MediatorNotification<>);
            var genericType = type.MakeGenericType(evt.GetType());
            var instance = Activator.CreateInstance(genericType, evt);
            await _mediator.Publish(instance!, cancellationToken);
        }
    }
}
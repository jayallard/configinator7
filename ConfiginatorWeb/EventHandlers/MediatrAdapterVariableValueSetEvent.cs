using Allard.Configinator.Core;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;

/// <summary>
/// The mediator notification handler for VariableValueSetEvent.
/// This forwards the events to all registered VariableValueSet event handlers.
/// </summary>
public class MediatrAdapterVariableValueSetEvent : INotificationHandler<MediatorNotification<VariableValueSetEvent>>
{
    private readonly List<IEventHandler<VariableValueSetEvent>> _handler;

    public MediatrAdapterVariableValueSetEvent(IEnumerable<IEventHandler<VariableValueSetEvent>> handler, IMediator mediator, IServiceScopeFactory scope)
    {
        _handler = Guards.HasValue(handler, nameof(handler)).ToList();
    }

    public async Task Handle(MediatorNotification<VariableValueSetEvent> notification, CancellationToken cancellationToken)
    {
        foreach (var handler in _handler)
        {
            await handler.ExecuteAsync(notification.DomainEvent, cancellationToken);
        }
    }
}
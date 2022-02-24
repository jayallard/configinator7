using Allard.Configinator.Core;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;

/// <summary>
///     The mediator notification handler for VariableValueSetEvent.
///     This forwards the events to all registered VariableValueSet event handlers.
/// </summary>
public class MediatrAdapterVariableValueSetEvent : INotificationHandler<MediatorNotification<VariableValueSetEvent>>
{
    private readonly List<IEventHandler<VariableValueSetEvent>> _handlers;

    public MediatrAdapterVariableValueSetEvent(IEnumerable<IEventHandler<VariableValueSetEvent>> handler)
    {
        _handlers = Guards.HasValue(handler, nameof(handler)).ToList();
    }

    public async Task Handle(
        MediatorNotification<VariableValueSetEvent> notification,
        CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers) 
            await handler.ExecuteAsync(notification.DomainEvent, cancellationToken);
    }
}
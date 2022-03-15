using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;

namespace ConfiginatorWeb.EventHandlers;

/// <summary>
///     The mediator notification handler for VariableValueSetEvent.
///     This forwards the events to all registered VariableValueSet event handlers.
/// </summary>
public class VariableSetNotificationHandler : NotificationHandlerBase<VariableSetCreatedEvent>
{
    public VariableSetNotificationHandler(IEnumerable<IDomainEventHandler<VariableSetCreatedEvent>> handlers) : base(handlers)
    {
    }
}
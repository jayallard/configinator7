using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;

namespace ConfiginatorWeb.EventHandlers;

public class VariableChangedNotificationHandler : NotificationHandlerBase<VariableValueSetEvent>
{
    public VariableChangedNotificationHandler(IEnumerable<IEventHandler<VariableValueSetEvent>> handlers) : base(handlers)
    {
    }
}
using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;

namespace ConfiginatorWeb.EventHandlers;

public class SectionCreatedNotificationHandler : NotificationHandlerBase<SectionCreatedEvent>
{
    public SectionCreatedNotificationHandler(IEnumerable<IDomainEventHandler<SectionCreatedEvent>> handlers) : base(handlers)
    {
    }
}
using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;

namespace ConfiginatorWeb.EventHandlers;

public class SchemaCreatedNotificationHandler : NotificationHandlerBase<SchemaCreatedEvent>
{
    public SchemaCreatedNotificationHandler(IEnumerable<IEventHandler<SchemaCreatedEvent>> handlers) : base(handlers)
    {
    }
}
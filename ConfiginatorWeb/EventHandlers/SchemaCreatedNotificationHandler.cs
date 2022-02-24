using Allard.Configinator.Core;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;
public class SchemaCreatedNotificationHandler  : INotificationHandler<MediatorNotification<SchemaCreatedEvent>>
{
    private readonly List<IEventHandler<SchemaCreatedEvent>> _handlers;

    public SchemaCreatedNotificationHandler(IEnumerable<IEventHandler<SchemaCreatedEvent>> handlers)
    {
        _handlers = handlers.ToList();
    }

    public async Task Handle(
        MediatorNotification<SchemaCreatedEvent> notification, 
        CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers) 
            await handler.ExecuteAsync(notification.DomainEvent, cancellationToken);
    }
}
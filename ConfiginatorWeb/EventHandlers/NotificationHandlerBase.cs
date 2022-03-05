using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;

public abstract class NotificationHandlerBase<T> : INotificationHandler<MediatorNotification<T>> where T : IDomainEvent
{
    private readonly List<IEventHandler<T>> _handlers;

    protected NotificationHandlerBase(IEnumerable<IEventHandler<T>> handlers)
    {
        _handlers = handlers.ToList();
    }

    public async Task Handle(MediatorNotification<T> notification, CancellationToken cancellationToken)
    {
        foreach (var handler in _handlers)
            await handler.ExecuteAsync(notification.DomainEvent, cancellationToken);
    }
}
using Allard.Configinator.Infrastructure;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;

public class ConsoleSubscriber<TNotification> : INotificationHandler<TNotification> 
    where TNotification : INotification
{
    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        var e = notification as MediatorNotificationBase;
        Console.WriteLine("=========================== received: " + e?.Event);
        return Task.CompletedTask;
    }
}
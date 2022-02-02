using MediatR;

namespace ConfiginatorWeb.EventHandlers;

public class ConsoleSubscriber<TNotification> : INotificationHandler<TNotification> 
    where TNotification : INotification
{
    public Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("=========================== received");
        return Task.CompletedTask;
    }
}
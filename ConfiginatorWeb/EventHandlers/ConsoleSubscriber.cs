// using Allard.Configinator.Core;
// using Allard.Configinator.Infrastructure;
// using MediatR;
//
// namespace ConfiginatorWeb.EventHandlers;
//
// public class ConsoleSubscriber<TNotification> : INotificationHandler<MediatorNotificationBase> 
//     where TNotification : INotification
// {
//     public Task Handle(TNotification notification, CancellationToken cancellationToken)
//     {
//         var e = notification as MediatorNotificationBase;
//         Console.WriteLine("=========================== received: " + notification);
//         return Task.CompletedTask;
//     }
//
//     public Task Handle(MediatorNotificationBase notification, CancellationToken cancellationToken)
//     {
//         var e = notification as MediatorNotificationBase;
//         Console.WriteLine("=========================== received 2: " + e);
//         return Task.CompletedTask;
//     }
// }
//
// public class Blah : INotificationHandler<MediatorNotification<TokenValueSetEvent>>
// {
//     public Task Handle(MediatorNotification<TokenValueSetEvent> notification, CancellationToken cancellationToken)
//     {
//         Console.WriteLine("--------- 1 " + notification.DomainEvent.TokenSetName + " : " + notification.DomainEvent.TokenName);
//         return Task.CompletedTask;
//     }
// }
//
// public class Blah2 : INotificationHandler<MediatorNotification<TokenValueSetEvent>>
// {
//     public Task Handle(MediatorNotification<TokenValueSetEvent> notification, CancellationToken cancellationToken)
//     {
//         Console.WriteLine("--------- 2 " + notification.DomainEvent.TokenSetName + " : " + notification.DomainEvent.TokenName);
//         return Task.CompletedTask;
//     }
// }
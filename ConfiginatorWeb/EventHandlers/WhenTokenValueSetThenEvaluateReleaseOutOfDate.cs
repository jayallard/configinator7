using Allard.Configinator.Core;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;

public class
    WhenTokenValueSetThenEvaluateReleaseOutOfDate : INotificationHandler<MediatorNotification<TokenValueSetEvent>>
{
    private readonly IEventHandler<TokenValueSetEvent> _handler;

    public WhenTokenValueSetThenEvaluateReleaseOutOfDate(IEventHandler<TokenValueSetEvent> handler) =>
        _handler = Guards.NotDefault(handler, nameof(handler));

    public Task Handle(MediatorNotification<TokenValueSetEvent> notification, CancellationToken cancellationToken) =>
        _handler.ExecuteAsync(notification.DomainEvent, cancellationToken);
}
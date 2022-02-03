﻿using Allard.Configinator.Core;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;

namespace ConfiginatorWeb.EventHandlers;

/// <summary>
/// The mediator notification handler for TokenValueSetEvent.
/// This forwards the events to all registered TokenValueSet event handlers.
/// </summary>
public class MediatrAdapterTokenValueSetEvent : INotificationHandler<MediatorNotification<TokenValueSetEvent>>
{
    private readonly List<IEventHandler<TokenValueSetEvent>> _handler;

    public MediatrAdapterTokenValueSetEvent(IEnumerable<IEventHandler<TokenValueSetEvent>> handler, IMediator mediator, IServiceScopeFactory scope)
    {
        _handler = Guards.NotDefault(handler, nameof(handler)).ToList();
    }

    public async Task Handle(MediatorNotification<TokenValueSetEvent> notification, CancellationToken cancellationToken)
    {
        foreach (var handler in _handler)
        {
            await handler.ExecuteAsync(notification.DomainEvent, cancellationToken);
        }
    }
}
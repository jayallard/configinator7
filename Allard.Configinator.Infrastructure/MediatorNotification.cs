using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Infrastructure;

/// <summary>
/// Core isn't aware of Mediator.
/// All of the events are in Core.
/// When IDomainEvents are published using MediatorPublisher,
/// MediatorPublisher wraps each event with this.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MediatorNotification<T> : INotification where T : IDomainEvent
{
    public MediatorNotification(T evt) => DomainEvent = evt;
    public T DomainEvent { get; }
}

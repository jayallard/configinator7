using Allard.DomainDrivenDesign;
using MediatR;

namespace Allard.Configinator.Infrastructure;

public abstract class MediatorNotificationBase : INotification
{
    protected MediatorNotificationBase(IDomainEvent evt)
    {
        Event = evt;
    }

    public IDomainEvent Event { get; }
}

/// <summary>
///     Core isn't aware of Mediator.
///     All of the events are in Core.
///     When IDomainEvents are published using MediatorPublisher,
///     MediatorPublisher wraps each event with this.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MediatorNotification<T> : MediatorNotificationBase
    where T : IDomainEvent
{
    public MediatorNotification(T evt) : base(evt)
    {
        DomainEvent = evt;
    }

    public T DomainEvent { get; }
}
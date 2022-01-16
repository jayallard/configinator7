using Allard.Configinator.Core;
using MediatR;

namespace Allard.Configinator.Infrastructure;

public class MediatorNotification<T> : INotification where T : IDomainEvent
{
    public MediatorNotification(T evt) => DomainEvent = evt;
    public T DomainEvent { get; }
}

using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public interface IEntity<TIdentity> where TIdentity : IIdentity
{
    TIdentity Id { get; }
    IEnumerable<ISourceEvent> SourceEvents { get; }
    IEnumerable<IDomainEvent> DomainEvents { get; }
}
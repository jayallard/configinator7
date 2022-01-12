using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public abstract class EntityBase<TIdentity> : IEntity<TIdentity> where TIdentity : IIdentity
{
    protected List<ISourceEvent> InternalSourceEvents { get; } = new();
    protected List<IDomainEvent> InternalDomainEvents { get; }= new();

    public TIdentity Id { get; }
    public IEnumerable<ISourceEvent> SourceEvents => InternalSourceEvents.AsReadOnly();
    public IEnumerable<IDomainEvent> DomainEvents => InternalDomainEvents.AsReadOnly();
    
    protected EntityBase(TIdentity id) => Id = Guards.NotDefault(id, nameof(id));
}
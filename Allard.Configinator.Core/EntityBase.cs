using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public abstract class EntityBase<TIdentity> : IEntity<TIdentity> where TIdentity : IIdentity
{
    private readonly List<ISourceEvent> _sourceEvents = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    public IEnumerable<ISourceEvent> SourceEvents => _sourceEvents.AsReadOnly();
    public IEnumerable<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public TIdentity Id { get; }

    protected EntityBase(TIdentity id) => Id = id;
}
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core;

public abstract class AggregateBase<TIdentity> : EntityBase<TIdentity>, IAggregate
    where TIdentity : IIdentity
{
    protected AggregateBase(TIdentity id) : base(id)
    {
    }

    protected List<IDomainEvent> InternalSourceEvents { get; } = new();
    public IEnumerable<IDomainEvent> SourceEvents => InternalSourceEvents.AsReadOnly();
    public void ClearEvents()
    {
        // TODO: see the notes on the interface
        InternalSourceEvents.Clear();
    }
}
namespace Allard.Configinator.Core;

public abstract class AggregateBase<TIdentity> : EntityBase<TIdentity>, IAggregate<TIdentity>
    where TIdentity : IIdentity
{
    protected AggregateBase(TIdentity id) : base(id)
    {
    }

    protected List<ISourceEvent> InternalSourceEvents { get; } = new();
    public IEnumerable<ISourceEvent> SourceEvents => InternalSourceEvents.AsReadOnly();
}
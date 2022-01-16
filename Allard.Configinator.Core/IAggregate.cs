namespace Allard.Configinator.Core;

public interface IAggregate<TIdentity> : IEntity<TIdentity> where TIdentity : IIdentity
{
    IEnumerable<IDomainEvent> SourceEvents { get; }

    // TODO: this could be a problem. if something calls it that shouldn't, then
    // the events are lost. as long as the aggregate is populated from events,
    // that's fine... when it reloads, it will be correct.
    // but if we snapshot on the current state, and the events that created that state
    // are lost, then... ick.
    void ClearEvents();
}
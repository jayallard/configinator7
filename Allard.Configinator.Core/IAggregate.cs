namespace Allard.Configinator.Core;

public interface IAggregate<TIdentity> : IEntity<TIdentity> where TIdentity : IIdentity
{
    IEnumerable<ISourceEvent> SourceEvents { get; }
}
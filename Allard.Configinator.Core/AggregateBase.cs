using System.Text.Json.Serialization;
using Allard.DomainDrivenDesign;
namespace Allard.Configinator.Core;

public abstract class AggregateBase<TIdentity> : EntityBase<TIdentity>, IAggregate
    where TIdentity : IIdentity
{
    [JsonIgnore]
    protected List<IDomainEvent> InternalSourceEvents { get; } = new();
    
    [JsonIgnore]
    public IEnumerable<IDomainEvent> SourceEvents => InternalSourceEvents.AsReadOnly();

    public void ClearEvents()
    {
        InternalSourceEvents.Clear();
    }
}
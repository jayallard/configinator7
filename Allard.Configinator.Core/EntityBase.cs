using System.Text.Json.Serialization;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core;

public abstract class EntityBase<TIdentity> : IEntity
    where TIdentity : IIdentity
{
    [JsonInclude]
    public TIdentity Id { get; internal set; }
    
    [JsonIgnore]
    public long EntityId => Id.Id;
}
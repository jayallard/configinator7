using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core;

public abstract class EntityBase<TIdentity> : IEntity
    where TIdentity : IIdentity
{
    public TIdentity? Id { get; internal set; }
    public long EntityId => Id == null ? -1 : Id.Id;
}
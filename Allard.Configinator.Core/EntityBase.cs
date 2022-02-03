using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core;

public abstract class EntityBase<TIdentity> : IEntity
    where TIdentity : IIdentity
{
    public TIdentity Id { get; internal set; }
    
    protected EntityBase(TIdentity id) => Id = Guards.NotDefault(id, nameof(id));
    public long EntityId => Id.Id;
}
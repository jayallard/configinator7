namespace Allard.Configinator.Core;

public abstract class EntityBase<TIdentity> : IEntity<TIdentity> where TIdentity : IIdentity
{
    public TIdentity Id { get; }
    
    protected EntityBase(TIdentity id) => Id = Guards.NotDefault(id, nameof(id));
}
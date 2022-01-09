namespace Allard.Configinator.Core;

public abstract class EntityBase<T, TId> : IEntity<T, TId>
{
    public TId Id { get; }

    protected EntityBase(TId id) => Id = id;
}
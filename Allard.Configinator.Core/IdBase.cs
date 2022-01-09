namespace Allard.Configinator.Core;

public class IdBase<T, TId> : IId<T, TId> where T: IEntity<T, TId>
{
    public long Id { get; }

    protected IdBase(long id) => Id = id;
}
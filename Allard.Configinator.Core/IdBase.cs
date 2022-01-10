namespace Allard.Configinator.Core;

public class IdBase<TEntity> : IIdentity
{
    public long Id { get; }

    protected IdBase(long id) => Id = id;
}
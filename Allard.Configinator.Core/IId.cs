namespace Allard.Configinator.Core;

public interface IId<T, TId> where T : IEntity<T, TId>
{
    public long Id { get; }
}
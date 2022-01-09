namespace Allard.Configinator.Core;

public interface IEntity<T, TId>
{
    public TId Id { get; }
}

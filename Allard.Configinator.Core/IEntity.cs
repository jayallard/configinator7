namespace Allard.Configinator.Core;

public interface IEntity<TIdentity> where TIdentity : IIdentity
{
    TIdentity Id { get; }
}
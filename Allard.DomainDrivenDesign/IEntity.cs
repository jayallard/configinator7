namespace Allard.DomainDrivenDesign;

public interface IEntity<TIdentity> where TIdentity : IIdentity
{
    TIdentity Id { get; }
}
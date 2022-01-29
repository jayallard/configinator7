namespace Allard.DomainDrivenDesign;

public interface IDataChangeTracker<TAggregate, in TIdentity> 
    where TAggregate : IAggregate
    where TIdentity : IIdentity
{
    Task<bool> Exists(ISpecification<TAggregate> specification);
    Task<List<TAggregate>> FindAsync(ISpecification<TAggregate> specification);
    Task AddAsync(TAggregate entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TAggregate?> GetAsync(TIdentity id, CancellationToken cancellationToken = default);
}
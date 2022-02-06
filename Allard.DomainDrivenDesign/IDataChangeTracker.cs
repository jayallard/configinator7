namespace Allard.DomainDrivenDesign;

public interface IDataChangeTracker<TAggregate, in TIdentity> 
    where TAggregate : IAggregate
    where TIdentity : IIdentity
{
    Task<bool> Exists(ISpecification<TAggregate> specification);
    Task<List<TAggregate>> FindAsync(ISpecification<TAggregate> specification, CancellationToken cancellationToken);
    Task AddAsync(TAggregate entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TAggregate> GetAsync(TIdentity id, CancellationToken cancellationToken = default);
    Task<List<IDomainEvent>> GetEvents(CancellationToken cancellationToken = default);
}
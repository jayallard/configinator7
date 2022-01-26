namespace Allard.DomainDrivenDesign;

public interface IDataChangeTracker<TEntity, TIdentity> where TEntity : IAggregate<TIdentity> where TIdentity : IIdentity
{
    Task<bool> Exists(ISpecification<TEntity> specification);
    Task<List<TEntity>> FindAsync(ISpecification<TEntity> specification);
    Task AddAsync(TEntity entity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetAsync(TIdentity id, CancellationToken cancellationToken = default);
}
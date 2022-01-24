namespace Allard.DomainDrivenDesign;

public interface IRepository<TEntity, in TIdentity> where TEntity : IAggregate<TIdentity> where TIdentity : IIdentity
{
    Task<TEntity?> GetAsync(TIdentity id, CancellationToken cancellationToken);

    Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> specification);

    Task<bool> ExistsAsync(ISpecification<TEntity> specification);
    
    Task SaveAsync(TEntity section);
}
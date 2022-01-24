using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Repositories;

public class RepositoryMemoryBase<TEntity, TIdentity> : IRepository<TEntity, TIdentity>  
    where TEntity : IAggregate<TIdentity> 
    where TIdentity : IIdentity
{

    private readonly Dictionary<TIdentity, TEntity> _database = new();
    
    public Task<TEntity?> GetAsync(TIdentity id, CancellationToken cancellationToken)
    {
        var section = (TEntity?)_database[id];
        return Task.FromResult(section);
    }

    public Task<IEnumerable<TEntity>> FindAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_database.Values.Where(specification.IsSatisfied));
    }

    public Task<bool> ExistsAsync(ISpecification<TEntity> specification)
    {
        return Task.FromResult(_database.Values.Any(specification.IsSatisfied));
    }

    public Task SaveAsync(TEntity entity)
    {
        _database[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
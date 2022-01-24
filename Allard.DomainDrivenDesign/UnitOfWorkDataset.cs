namespace Allard.DomainDrivenDesign;

public class UnitOfWorkDataset<TEntity, TIdentity>
    where TEntity : IAggregate<TIdentity>
    where TIdentity : IIdentity
{
    private readonly List<TEntity> _localData = new();
    private readonly IRepository<TEntity, TIdentity> _repository;

    public UnitOfWorkDataset(IRepository<TEntity, TIdentity> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Exists(ISpecification<TEntity> specification)
        => _localData.Any(specification.IsSatisfied) || await _repository.ExistsAsync(specification);

    public async Task<List<TEntity>> FindAsync(ISpecification<TEntity> specification)
    {
        var notAlreadyInMemory = (ISpecification<TEntity>) new IdNotIn<TIdentity>(_localData.Select(s => s.Id));
        var fromDb = await _repository.FindAsync(notAlreadyInMemory.And(specification));

        // add the new ones to memory.
        _localData.AddRange(fromDb);

        // execute the query against memory, which now has everything we need
        return _localData.Where(specification.IsSatisfied).ToList();
    }

    public Task AddAsync(TEntity entity)
    {
        // todo: exception if already exists
        _localData.Add(entity);
        return Task.CompletedTask;
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var e in _localData)
        {
            await _repository.SaveAsync(e);
        }
        
        _localData.Clear();
    }
}
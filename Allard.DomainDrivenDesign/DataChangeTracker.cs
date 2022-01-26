namespace Allard.DomainDrivenDesign;

public class DataChangeTracker<TEntity, TIdentity> : IDataChangeTracker<TEntity, TIdentity>
    where TEntity : IAggregate<TIdentity>
    where TIdentity : IIdentity
{
    private readonly List<TEntity> _localData = new();
    private readonly IRepository<TEntity, TIdentity> _repository;

    public DataChangeTracker(IRepository<TEntity, TIdentity> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Exists(ISpecification<TEntity> specification)
        => _localData.Any(specification.IsSatisfied) || await _repository.ExistsAsync(specification);

    public async Task<List<TEntity>> FindAsync(ISpecification<TEntity> specification)
    {
        // TODO: couldn't get the types working for this. TIdentity is a pain. Skip for now.
        //var notAlreadyInMemory = (ISpecification<TEntity>) new IdNotIn<TIdentity>(_localData.Select(s => s.Id));
        var fromDb = (await _repository.FindAsync(specification)).ToList();

        // inefficient. got all matches from the db, now remove those that we already 
        // have locally.
        var ids = _localData.Select(d => d.Id.Id).ToHashSet();
        var withoutLocal = fromDb.Where(e => !ids.Contains(e.Id.Id));

        // add the new ones to memory.
        _localData.AddRange(withoutLocal);

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

    public async Task<TEntity?> GetAsync(TIdentity id, CancellationToken cancellationToken = default) =>
        await _repository.GetAsync(id, cancellationToken);
}
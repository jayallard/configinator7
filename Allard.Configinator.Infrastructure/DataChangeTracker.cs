using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class DataChangeTracker<TAggregate, TIdentity> : IDataChangeTracker<TAggregate, TIdentity>
    where TAggregate : IAggregate
    where TIdentity : IIdentity
{
    private readonly List<TAggregate> _localData = new();
    private readonly IRepository<TAggregate, TIdentity> _repository;

    public DataChangeTracker(IRepository<TAggregate, TIdentity> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Exists(ISpecification<TAggregate> specification)
        => _localData.Any(specification.IsSatisfied) || await _repository.ExistsAsync(specification);

    public async Task<List<TAggregate>> FindAsync(ISpecification<TAggregate> specification)
    {
        var notAlreadyInMemory = new IdNotIn(_localData.Select(s => s.EntityId));
        var a = new AndSpecification<IAggregate, TAggregate>(notAlreadyInMemory, specification);
        var fromDbTask = await _repository.FindAsync(a);
        var fromDb = fromDbTask.ToList();

        // inefficient. got all matches from the db, now remove those that we already 
        // have locally.
        var ids = _localData.Select(d => d.EntityId).ToHashSet();
        var withoutLocal = fromDb.Where(e => !ids.Contains(e.EntityId));

        // add the new ones to memory.
        _localData.AddRange(withoutLocal);

        // execute the query against memory, which now has everything we need
        return _localData.Where(specification.IsSatisfied).ToList();
    }

    public Task AddAsync(TAggregate entity)
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

    public async Task<TAggregate?> GetAsync(TIdentity id, CancellationToken cancellationToken = default) =>
        await _repository.GetAsync(id, cancellationToken);
}
using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public sealed class DataChangeTracker<TAggregate, TIdentity> : IDataChangeTracker<TAggregate, TIdentity>, IDisposable
    where TAggregate : IAggregate
    where TIdentity : IIdentity
{
    private readonly List<TAggregate> _localData = new();
    private readonly IRepository<TAggregate, TIdentity> _repository;

    public DataChangeTracker(IRepository<TAggregate, TIdentity> repository)
    {
        _repository = Guards.HasValue(repository, nameof(repository));
    }

    public async Task<bool> Exists(ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        return _localData.Any(specification.IsSatisfied) ||
               await _repository.ExistsAsync(specification, cancellationToken);
    }

    public async Task<List<TAggregate>> FindAsync(ISpecification<TAggregate> specification,
        CancellationToken cancellationToken)
    {
        // if it's already in memory, don't get it from the db
        var notAlreadyInMemory = new IdNotIn(_localData.Select(s => s.EntityId));

        // concat the IdNotIn with the passed-in specification.
        var a = new AndSpecification<IAggregate, TAggregate>(notAlreadyInMemory, specification);

        // execute the query
        var fromDbTask = await _repository.FindAsync(a, cancellationToken);
        var fromDb = fromDbTask.ToList();

        // add the new ones to memory.
        _localData.AddRange(fromDb);

        // execute the query against memory, which now has everything we need
        return _localData.Where(specification.IsSatisfied).ToList();
    }

    public Task AddAsync(TAggregate entity, CancellationToken cancellationToken = default)
    {
        if (_localData.Any(d => d.EntityId == entity.EntityId))
            throw new InvalidOperationException("The entity aggregate has already been added.");
        _localData.Add(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var e in _localData)
        {
            await _repository.SaveAsync(e, cancellationToken);
            e.ClearEvents();
        }

        _localData.Clear();
    }

    public async Task<TAggregate> GetAsync(TIdentity id, CancellationToken cancellationToken = default)
    {
        var local = _localData.SingleOrDefault(d => d.EntityId == id.Id);
        if (local != null) return local;
        var db = await _repository.GetAsync(id, cancellationToken);
        if (db == null) throw new InvalidOperationException($"{id.GetType().Name} doesn't exist. Id: " + id.Id);
        _localData.Add(db);
        return db;
    }

    public async Task<TAggregate> FindOneAsync(ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        var matches = await FindAsync(specification, cancellationToken);
        if (matches.Count > 1) throw new InvalidOperationException("The query returned multiple matches");
        if (matches.Count == 0) throw new InvalidOperationException("The query didn't return a result   ");
        return matches[0];
    }

    public Task<List<IDomainEvent>> GetEvents(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_localData.SelectMany(d => d.SourceEvents).ToList());
    }

    public void Dispose()
    {
        _localData.Clear();
    }
}
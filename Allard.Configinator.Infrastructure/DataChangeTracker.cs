using Allard.Configinator.Core;
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
        _repository = Guards.NotDefault(repository, nameof(repository));
    }

    public async Task<bool> Exists(ISpecification<TAggregate> specification)
        => _localData.Any(specification.IsSatisfied) || await _repository.ExistsAsync(specification);

    public async Task<List<TAggregate>> FindAsync(ISpecification<TAggregate> specification, CancellationToken cancellationToken)
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

    public Task<List<IDomainEvent>> GetEvents(CancellationToken cancellationToken = default) => Task.FromResult(_localData.SelectMany(d => d.SourceEvents).ToList());
}
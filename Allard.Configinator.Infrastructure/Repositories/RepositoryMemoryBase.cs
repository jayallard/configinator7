using System.Diagnostics;
using System.Reflection;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Repositories;

public class RepositoryMemoryBase<TAggregate, TIdentity> : IRepository<TAggregate, TIdentity>
    where TAggregate : IAggregate
    where TIdentity : IIdentity
{
    private readonly Dictionary<long, List<IDomainEvent>> _events = new();
    private readonly Dictionary<long, TAggregate> _snapshots = new();

    public Task<TAggregate?> GetAsync(TIdentity id, CancellationToken cancellationToken)
    {
        return GetAsync(id.Id, cancellationToken);
    }

    public async Task<IEnumerable<TAggregate>> FindAsync(
        ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        var entities = _snapshots.Values.Where(specification.IsSatisfied);
        var copies = new List<TAggregate>();
        foreach (var e in entities)
        {
            var copy = await GetAsync(e.EntityId, cancellationToken);
            if (copy == null) throw new InvalidOperationException("bug!");
            copies.Add(copy);
        }

        return copies;
    }

    public Task<bool> ExistsAsync(ISpecification<TAggregate> specification,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_snapshots.Values.Any(specification.IsSatisfied));
    }

    public async Task SaveAsync(TAggregate entity, CancellationToken cancellationToken)
    {
        if (!_events.ContainsKey(entity.EntityId)) _events[entity.EntityId] = new List<IDomainEvent>();

        // add the events to the event stream
        _events[entity.EntityId].AddRange(entity.SourceEvents);

        // create a new snapshot from all of the events
        var snapshot = await GetAsync(entity.EntityId, cancellationToken);
        Debug.Assert(snapshot != null);
        _snapshots[entity.EntityId] = snapshot;
    }

    private Task<TAggregate?> GetAsync(long id, CancellationToken cancellationToken)
    {
        if (!_events.ContainsKey(id)) return Task.FromResult(default(TAggregate?));
        var events = _events[id];
        var entity = (TAggregate) Activator.CreateInstance(typeof(TAggregate),
            BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] {events}, null)!;
        return Task.FromResult(entity)!;
    }
}
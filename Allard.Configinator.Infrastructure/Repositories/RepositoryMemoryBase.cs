using System.Diagnostics;
using System.Reflection;
using Allard.Configinator.Core;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Repositories;

public class RepositoryMemoryBase<TAggregate, TIdentity> : IRepository<TAggregate, TIdentity>
    where TAggregate : IAggregate
    where TIdentity : IIdentity
{
    // key = entity id
    private readonly Dictionary<long, List<EventStorageRecord>> _events = new();

    // key = entity id
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

    public async Task SaveAsync(TransactionContext txContext, TAggregate aggregate, CancellationToken cancellationToken)
    {
        if (!_events.ContainsKey(aggregate.EntityId)) _events[aggregate.EntityId] = new List<EventStorageRecord>();

        // add the events to the event stream
        var streamType = aggregate.GetType().FullName;
        var eventsRecords = aggregate
            .SourceEvents
            .Select(e => new EventStorageRecord
            {
                EventType = e.GetType().FullName,
                Event = ModelJsonUtility.Serialize(e),
                AggregateId = 0,
                StreamType = streamType,
                TransactionId = txContext.TransactionId.ToString()
            })
            .ToList();
        _events[aggregate.EntityId].AddRange(eventsRecords);

        // create a new snapshot from all of the events
        var snapshot = await GetAsync(aggregate.EntityId, cancellationToken);
        Debug.Assert(snapshot != null);
        _snapshots[aggregate.EntityId] = snapshot;
    }

    private Task<TAggregate?> GetAsync(long id, CancellationToken cancellationToken)
    {
        if (!_events.ContainsKey(id)) return Task.FromResult(default(TAggregate?));
        var events = _events[id]
            .Select(e =>
            {
                try
                {
                    var type = Type.GetType(e.EventType + ", Allard.Configinator.Core");
                    var evt = ModelJsonUtility.Deserialize<IDomainEvent>(type, e.Event);
                    return evt;
                }
                catch (Exception)
                {
                    var x = _events;
                    throw;
                }
            })
            .ToList();


        var entity = (TAggregate) Activator.CreateInstance(typeof(TAggregate),
            BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] {events}, null)!;
        return Task.FromResult(entity)!;
    }
}

public class EventStorageRecord
{
    public string StreamType { get; set; }
    public string EventType { get; set; }
    public string TransactionId { get; set; }
    public long AggregateId { get; set; }
    public string Event { get; set; }
}
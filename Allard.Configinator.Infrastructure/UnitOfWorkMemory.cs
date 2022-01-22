using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class UnitOfWorkMemory : IUnitOfWork
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IEventSourceRepository _eventSourceRepository;
    private readonly DatabaseMemory _databaseMemory;
    private ISectionRepository Sections { get; }

    public UnitOfWorkMemory(ISectionRepository sectionRepository, IEventPublisher eventPublisher,
        IEventSourceRepository eventSourceRepository, DatabaseMemory databaseMemory)
    {
        Sections = sectionRepository;
        _eventPublisher = eventPublisher;
        _eventSourceRepository = eventSourceRepository;
        _databaseMemory = databaseMemory;
    }

    private async Task PublishSourceEventsFrom<TEntity, TIdentity>(IEnumerable<IAggregate<TIdentity>> entities,
        CancellationToken token)
        where TIdentity : IIdentity
    {
        var type = typeof(TEntity);
        foreach (var e in entities)
        {
            await _eventSourceRepository.AppendAsync(type, e.Id.Id, e.SourceEvents, token);
            await _eventPublisher.PublishAsync(e.SourceEvents, token);
            e.ClearEvents();
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var entities = _databaseMemory.Sections.Values.AsEnumerable();
        await PublishSourceEventsFrom<SectionEntity, SectionId>(entities, cancellationToken);
    }
}
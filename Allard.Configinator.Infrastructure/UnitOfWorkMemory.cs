using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure;

public class UnitOfWorkMemory : IUnitOfWork
{
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IEventSourceRepository _eventSourceRepository;
    private readonly DatabaseMemory _databaseMemory;

    public ISectionRepository Sections { get; }

    public UnitOfWorkMemory(ISectionRepository sectionRepository, IDomainEventPublisher domainEventPublisher,
        IEventSourceRepository eventSourceRepository, DatabaseMemory databaseMemory)
    {
        Sections = sectionRepository;
        _domainEventPublisher = domainEventPublisher;
        _eventSourceRepository = eventSourceRepository;
        _databaseMemory = databaseMemory;
    }

    private Task PublishSourceEventsFrom<TEntity, TIdentity>(IEnumerable<IEntity<TIdentity>> entities) where TIdentity : IIdentity
    {
        var type = typeof(TEntity);
        foreach (var e in entities)
        {
            _eventSourceRepository.AppendAsync(type, e.Id.Id, e.SourceEvents);
        }

        return Task.CompletedTask;
    }


    public Task SaveAsync()
    {
        PublishSourceEventsFrom<SectionEntity, SectionId>(_databaseMemory.Sections.Values.AsEnumerable());
        return Task.CompletedTask;
    }
}
using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Repositories;

public class UnitOfWorkMemory : IUnitOfWork, IDisposable
{
    private readonly IEventPublisher _publisher;

    private bool _disposed;

    public UnitOfWorkMemory(
        ISectionRepository sectionRepository,
        IVariableSetRepository variableSetRepository,
        ISchemaRepository schemaRepository,
        INamespaceRepository namespaceRepository,
        IEventPublisher publisher)
    {
        _publisher = Guards.HasValue(publisher, nameof(publisher));
        Sections = new DataChangeTracker<SectionAggregate, SectionId>(sectionRepository);
        VariableSets = new DataChangeTracker<VariableSetAggregate, VariableSetId>(variableSetRepository);
        Schemas = new DataChangeTracker<SchemaAggregate, SchemaId>(schemaRepository);
        Namespaces = new DataChangeTracker<NamespaceAggregate, NamespaceId>(namespaceRepository);
    }

    public void Dispose()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UnitOfWorkMemory));
        _disposed = true;
        (Sections as IDisposable)?.Dispose();
        (VariableSets as IDisposable)?.Dispose();
        (Schemas as IDisposable)?.Dispose();
    }

    public IDataChangeTracker<SectionAggregate, SectionId> Sections { get; }
    public IDataChangeTracker<VariableSetAggregate, VariableSetId> VariableSets { get; }
    public IDataChangeTracker<SchemaAggregate, SchemaId> Schemas { get; }
    public IDataChangeTracker<NamespaceAggregate, NamespaceId> Namespaces { get; }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var events =
            (await Sections.GetEvents(cancellationToken))
            .Concat(await VariableSets.GetEvents(cancellationToken))
            .Concat(await Schemas.GetEvents(cancellationToken))
            .OrderBy(e => e.EventDate)
            .ToList();

        // write the changes, then publish events.
        await Sections.SaveChangesAsync(cancellationToken);
        await VariableSets.SaveChangesAsync(cancellationToken);
        await Schemas.SaveChangesAsync(cancellationToken);
        await Namespaces.SaveChangesAsync(cancellationToken);

        // this is after the commit. if this fails, then data changed and
        // downstream systems won't get word.
        // alternative: publish to the db outbox before the commit,
        // then copy from outbox to publisher.
        await _publisher.PublishAsync(events, cancellationToken);
    }
}
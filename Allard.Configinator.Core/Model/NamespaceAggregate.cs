using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class NamespaceAggregate : AggregateBase<NamespaceId>
{
    private readonly ISet<SchemaId> _schemas = new HashSet<SchemaId>();
    private readonly ISet<SectionId> _sections = new HashSet<SectionId>();
    private readonly ISet<VariableSetId> _variableSets = new HashSet<VariableSetId>();

    internal NamespaceAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) Play(evt);
        InternalSourceEvents.Clear();
    }

    public NamespaceAggregate(NamespaceId namespaceId, string @namespace)
    {
        Play(new NamespaceCreatedEvent(namespaceId, @namespace));
    }

    public ISet<SchemaId> Schemas => _schemas.ToHashSet();
    public ISet<VariableSetId> VariableSets => _variableSets.ToHashSet();
    public ISet<SectionId> Sections => _sections.ToHashSet();
    public string Namespace { get; private set; }

    internal void AddSchema(SchemaId schemaId)
    {
        Play(new SchemaAddedToNamespaceEvent(Id, schemaId));
    }

    internal void AddVariableSet(VariableSetId variableSetId)
    {
        Play(new VariableSetAddedToNamespaceEvent(Id, variableSetId));
    }

    internal void AddSection(SectionId sectionId)
    {
        Play(new SectionAddedToNamespaceEvent(Id, sectionId));
    }

    private void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case NamespaceCreatedEvent created:
                Id = created.NamespaceId;
                Namespace = created.Namespace;
                break;
            case SchemaAddedToNamespaceEvent schemaAdded:
                _schemas.Add(schemaAdded.SchemaId);
                break;
            case VariableSetAddedToNamespaceEvent variableSetAdded:
                _variableSets.Add(variableSetAdded.SchemaId);
                break;
            case SectionAddedToNamespaceEvent sectionAdded:
                _sections.Add(sectionAdded.SchemaId);
                break;
            default:
                throw new InvalidOperationException("Unhandled event: " + evt.GetType().FullName);
        }
    }
}

public record NamespaceCreatedEvent(NamespaceId NamespaceId, string Namespace) : DomainEventBase;

public record SchemaAddedToNamespaceEvent(NamespaceId NamespaceId, SchemaId SchemaId) : DomainEventBase;

public record VariableSetAddedToNamespaceEvent(NamespaceId NamespaceId, VariableSetId SchemaId) : DomainEventBase;

public record SectionAddedToNamespaceEvent(NamespaceId NamespaceId, SectionId SchemaId) : DomainEventBase;
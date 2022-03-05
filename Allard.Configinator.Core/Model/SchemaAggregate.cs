using System.Text.Json;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class SchemaAggregate : AggregateBase<SchemaId>
{
    private readonly HashSet<string> _environmentTypes = new();

    internal SchemaAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) Play(evt);
        InternalSourceEvents.Clear();
    }

    internal SchemaAggregate(
        SchemaId schemaId,
        SectionId? sectionId,
        string environmentType,
        string @namespace,
        SchemaName name,
        string? description,
        JsonDocument schema)
    {
        Guards.HasValue(schemaId, nameof(schemaId));
        Guards.HasValue(name, nameof(name));
        Guards.HasValue(schema, nameof(schema));
        Play(new SchemaCreatedEvent(schemaId, sectionId, @namespace, name, description, environmentType, schema));
    }

    public SectionId? SectionId { get; private set; }
    public string Namespace { get; private set; }
    public string? Description { get; private set; }
    public IEnumerable<string> EnvironmentTypes => _environmentTypes.ToList();
    public SchemaName SchemaName { get; private set; }
    public JsonDocument Schema { get; private set; }

    internal void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case SchemaCreatedEvent created:
                Id = created.SchemaId;
                SectionId = created.SectionId;
                SchemaName = created.Name;
                Schema = created.Schema;
                Description = created.Description;
                SectionId = created.SectionId;
                Namespace = created.Namespace;
                _environmentTypes.Add(created.EnvironmentType);
                break;
            case SchemaPromotedEvent promoted:
                _environmentTypes.Add(promoted.ToEnvironmentType);
                break;
            default:
                throw new InvalidOperationException("Unhandled event type: " + evt.GetType().FullName);
        }
    }
}
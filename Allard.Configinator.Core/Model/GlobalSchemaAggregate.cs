using System.Text.Json;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class GlobalSchemaAggregate : AggregateBase<SchemaId>
{
    private readonly HashSet<string> _environmentTypes = new();

    internal GlobalSchemaAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) Play(evt);
        InternalSourceEvents.Clear();
    }


    internal GlobalSchemaAggregate(
        SchemaId schemaId,
        SectionId? sectionId,
        string environmentType,
        string name,
        string? description,
        JsonDocument schema)
    {
        Guards.HasValue(schemaId, nameof(schemaId));
        Guards.HasValue(name, nameof(name));
        Guards.HasValue(schema, nameof(schema));
        if (sectionId == null)
        {
            Play(new GlobalSchemaCreatedEvent(schemaId, name, description, environmentType, schema));
        }
        else
        {
            Play(new SectionSchemaCreatedEvent(schemaId, sectionId, name, description, environmentType, schema));
        }
    }

    public SectionId? SectionId { get; private set; }

    public bool IsGlobalSchema => SectionId == null;

    public string? Description { get; private set; }
    public IEnumerable<string> EnvironmentTypes => _environmentTypes.ToList();

    public string Name { get; private set; }
    public JsonDocument Schema { get; private set; }

    internal void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case GlobalSchemaCreatedEvent created:
                Id = created.SchemaId;
                Name = created.Name;
                Schema = created.Schema;
                Description = created.Description;
                _environmentTypes.Add(created.EnvironmentType);
                break;
            case GlobalSchemaPromotedEvent promoted:
                _environmentTypes.Add(promoted.ToEnvironmentType);
                break;
            default:
                throw new InvalidOperationException("Unhandled event type: " + evt.GetType().FullName);
        }
    }
}
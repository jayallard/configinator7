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
        SchemaName name,
        string? description,
        JsonDocument schema)
    {
        Guards.HasValue(schemaId, nameof(schemaId));
        Guards.HasValue(name, nameof(name));
        Guards.HasValue(schema, nameof(schema));
        if (sectionId == null)
            Play(new GlobalSchemaCreatedEvent(schemaId, name, description, environmentType, schema));
        else
            Play(new SectionSchemaCreatedEvent(schemaId, sectionId, name, description, environmentType, schema));
    }

    public SectionId? SectionId { get; private set; }

    public bool IsGlobalSchema => SectionId == null;

    public string? Description { get; private set; }
    public IEnumerable<string> EnvironmentTypes => _environmentTypes.ToList();

    public SchemaName SchemaName { get; private set; }
    public JsonDocument Schema { get; private set; }

    internal void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case GlobalSchemaCreatedEvent globalCreated:
                Id = globalCreated.SchemaId;
                SchemaName = globalCreated.Name;
                Schema = globalCreated.Schema;
                Description = globalCreated.Description;
                _environmentTypes.Add(globalCreated.EnvironmentType);
                break;
            case SectionSchemaCreatedEvent sectionCreated:
                Id = sectionCreated.SchemaId;
                SchemaName = sectionCreated.Name;
                Schema = sectionCreated.Schema;
                Description = sectionCreated.Description;
                SectionId = sectionCreated.SectionId;
                _environmentTypes.Add(sectionCreated.EnvironmentType);
                break;
            case GlobalSchemaPromotedEvent promoted:
                _environmentTypes.Add(promoted.ToEnvironmentType);
                break;
            default:
                throw new InvalidOperationException("Unhandled event type: " + evt.GetType().FullName);
        }
    }
}
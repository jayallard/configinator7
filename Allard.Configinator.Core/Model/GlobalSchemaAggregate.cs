using System.Text.Json;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class GlobalSchemaAggregate : AggregateBase<GlobalSchemaId>
{
    internal GlobalSchemaAggregate(List<IDomainEvent> events)
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) PlayEvent(evt);
        InternalSourceEvents.Clear();
    }

    internal GlobalSchemaAggregate(
        GlobalSchemaId id,
        string name,
        string? description,
        JsonDocument schema)
    {
        Guards.NotDefault(id, nameof(id));
        Guards.NotEmpty(name, nameof(name));
        Guards.NotDefault(schema, nameof(schema));
        PlayEvent(new GlobalSchemaCreated(id, name, description, schema));
    }

    public string? Description { get; private set; }
    public string Name { get; private set; }
    public JsonDocument Schema { get; private set; }
    private void PlayEvent(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case GlobalSchemaCreated created:
                Id = created.GlobalSchemaId;
                Name = created.Name;
                Schema = created.Schema;
                Description = created.Description;
                break;
            default:
                throw new InvalidOperationException("Unhandled event type: " + evt.GetType().FullName);
        }
    }
}
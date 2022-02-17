using System.Text.Json;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class GlobalSchemaAggregate : AggregateBase<GlobalSchemaId>
{
    private readonly HashSet<string> _environmentTypes = new();
    internal GlobalSchemaAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) PlayEvent(evt);
        InternalSourceEvents.Clear();
    }

    internal GlobalSchemaAggregate(
        GlobalSchemaId id,
        string environmentType,
        string name,
        string? description,
        JsonDocument schema)
    {
        Guards.HasValue(id, nameof(id));
        Guards.HasValue(name, nameof(name));
        Guards.HasValue(schema, nameof(schema));
        PlayEvent(new GlobalSchemaCreated(id, name, description, environmentType, schema));
    }

    public string? Description { get; private set; }
    public IEnumerable<string> EnvironmentTypes => _environmentTypes.ToList();
    
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
                _environmentTypes.Add(created.EnvironmentType);
                break;
            default:
                throw new InvalidOperationException("Unhandled event type: " + evt.GetType().FullName);
        }
    }
}
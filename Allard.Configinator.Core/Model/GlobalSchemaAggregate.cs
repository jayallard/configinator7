using Allard.DomainDrivenDesign;
using NJsonSchema;
using NuGet.Versioning;

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
        SemanticVersion version,
        JsonSchema schema)
    {
        Guards.NotDefault(id, nameof(id));
        Guards.NotEmpty(name, nameof(name));
        Guards.NotDefault(version, nameof(version));
        Guards.NotDefault(schema, nameof(schema));
        PlayEvent(new GlobalSchemaCreated(id, name, version, schema));
    }

    public string Name { get; private set; }
    public SemanticVersion Version { get; private set; }
    public JsonSchema Schema { get; private set; }

    private void PlayEvent(IDomainEvent evt)
    {
        switch (evt)
        {
            case GlobalSchemaCreated created:
                Id = created.GlobalSchemaId;
                Name = created.Name;
                Version = created.Version;
                Schema = created.Schema;
                break;
            default:
                throw new InvalidOperationException("Unhandled event type: " + evt.GetType().FullName);
        }
    }
}
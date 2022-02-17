using System.Text.Json;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionSchemaEntity : EntityBase<SectionSchemaId>
{
    internal HashSet<string> InternalEnvironmentTypes { get; } = new();
    public IEnumerable<string> EnvironmentTypes => InternalEnvironmentTypes.ToList();
    public string SchemaName { get; }
    public JsonDocument Schema { get; }
    internal SectionSchemaEntity(SectionSchemaId id, string name, JsonDocument schema, string environmentType)
    {
        Id = Guards.HasValue(id, nameof(id));
        SchemaName = Guards.HasValue(name, nameof(name));
        Schema = Guards.HasValue(schema, nameof(schema));
        
        Guards.HasValue(environmentType, nameof(environmentType));
        InternalEnvironmentTypes.Add(environmentType);
    }
}

using System.Text.Json;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionSchemaEntity : EntityBase<SectionSchemaId>
{
    internal HashSet<string> InternalEnvironmentTypes { get; } = new();
    public IEnumerable<string> EnvironmentTypes => InternalEnvironmentTypes.ToList();
    public SemanticVersion Version { get; }
    public JsonDocument Schema { get; }
    internal SectionSchemaEntity(SectionSchemaId id, SemanticVersion version, JsonDocument schema, string environmentType)
    {
        Id = Guards.HasValue(id, nameof(id));
        Version = Guards.HasValue(version, nameof(version));
        Schema = Guards.HasValue(schema, nameof(schema));
        
        Guards.HasValue(environmentType, nameof(environmentType));
        InternalEnvironmentTypes.Add(environmentType);
    }
}

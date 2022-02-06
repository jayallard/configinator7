using System.Text.Json;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionSchemaEntity : EntityBase<SectionSchemaId>
{
    public SemanticVersion Version { get; }
    public JsonDocument Schema { get; }

    public SectionSchemaEntity(SectionSchemaId id, SemanticVersion version, JsonDocument schema)
    {
        Id = id;
        Version = Guards.NotDefault(version, nameof(version));
        Schema = Guards.NotDefault(schema, nameof(schema));
    }
}

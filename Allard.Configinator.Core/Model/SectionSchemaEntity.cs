using NJsonSchema;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionSchemaEntity : EntityBase<SectionSchemaId>
{
    public SemanticVersion Version { get; }
    public JsonSchema Schema { get; }

    public SectionSchemaEntity(SectionSchemaId id, SemanticVersion version, JsonSchema schema)
    {
        Id = id;
        Version = Guards.NotDefault(version, nameof(version));
        Schema = Guards.NotDefault(schema, nameof(schema));
    }
}

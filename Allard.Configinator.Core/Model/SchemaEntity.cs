using NJsonSchema;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SchemaEntity : EntityBase<SchemaId>
{
    public SemanticVersion Version { get; }
    public JsonSchema Schema { get; }

    public SchemaEntity(SchemaId id, SemanticVersion version, JsonSchema schema) : base(id)
    {
        Version = Guards.NotDefault(version, nameof(version));
        Schema = Guards.NotDefault(schema, nameof(schema));
    }
}

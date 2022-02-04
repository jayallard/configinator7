using Newtonsoft.Json.Linq;

namespace Allard.Json.NJsonSchema;

public interface IJsonSchemaResolver
{
    Task<JObject> GetJsonSchema(SchemaId id, CancellationToken cancellationToken = default);
}
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Allard.Json.NJsonSchema;

public class SchemaLoader
{
    private readonly IJsonSchemaResolver _resolver;
    private readonly Dictionary<string, JsonSchema> _resolved = new();

    public SchemaLoader(IJsonSchemaResolver resolver)
    {
        _resolver = resolver;
    }

    private static List<string> GetSchemaReferences(JObject doc) =>
        doc.SelectTokens("$..['$ref']").Select(t => t.Value<string>()).ToList()!;

    public async Task<JsonSchema> LoadAsync(JObject jsonSchema, CancellationToken cancellationToken = default)
    {
        return await JsonSchema.FromJsonAsync(jsonSchema.ToString(), ".", Resolve, cancellationToken);
    }

    private JsonReferenceResolver Resolve(JsonSchema schema)
    {
        var references = GetSchemaReferences(JObject.Parse(schema.ToJson()));
        var schemaResolver = new JsonSchemaResolver(schema, new JsonSchemaGeneratorSettings());
        var referenceResolver = new JsonReferenceResolver(schemaResolver);
        foreach (var r in references)
        {
            if (!_resolved.ContainsKey(r))
            {
                // hack
                var referenceJson = _resolver.GetJsonSchema(new SchemaId(r)).Result;
                var referenceSchema = JsonSchema
                    .FromJsonAsync(referenceJson.ToString(), ".", Resolve, CancellationToken.None).Result;
                _resolved.Add(r, referenceSchema);
            }

            referenceResolver.AddDocumentReference(r, _resolved[r]);
        }
        
        return referenceResolver;
    }
}
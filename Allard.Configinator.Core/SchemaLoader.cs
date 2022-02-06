using System.Text.Json;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Allard.Configinator.Core;

public class SchemaLoader
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Dictionary<string, JsonSchema> _resolved = new();

    public SchemaLoader(IUnitOfWork unitOfWork) =>
        _unitOfWork = Guards.NotDefault(unitOfWork, nameof(unitOfWork));

    public Task<JsonSchema> GetSchemaAsync(
        JsonDocument schema,
        CancellationToken cancellationToken = default) =>
        JsonSchema.FromJsonAsync(schema.RootElement.ToString(), ".", Resolve, cancellationToken);

    private static List<string> GetSchemaReferences(JToken doc) =>
        doc.SelectTokens("$..['$ref']").Select(t => t.Value<string>()).ToList()!;

    private JsonReferenceResolver Resolve(JsonSchema schema)
    {
        // TODO: section level schemas may also be references
        var references = GetSchemaReferences(JObject.Parse(schema.ToJson()));
        var schemaResolver = new JsonSchemaResolver(schema, new JsonSchemaGeneratorSettings());
        var referenceResolver = new JsonReferenceResolver(schemaResolver);
        foreach (var r in references)
        {
            if (!_resolved.ContainsKey(r))
            {
                // hack - .RESULT
                var referenceJson = _unitOfWork.GlobalSchemas.FindAsync(new GlobalSchemaName(r), CancellationToken.None)
                    .Result.SingleOrDefault();
                if (referenceJson == null) throw new InvalidOperationException("GlobalSchema doesn't exist: " + r);

                var schemaRef = referenceJson.Schema.ToJsonNetJson();
                var referenceSchema = JsonSchema
                    .FromJsonAsync(schemaRef.ToString(), ".", Resolve, CancellationToken.None).Result;
                _resolved.Add(r, referenceSchema);
            }

            referenceResolver.AddDocumentReference(r, _resolved[r]);
        }

        return referenceResolver;
    }
}
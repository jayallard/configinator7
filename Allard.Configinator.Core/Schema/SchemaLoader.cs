using System.Text.Json;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Allard.Configinator.Core.Schema;

public class SchemaLoader
{
    private readonly IUnitOfWork _unitOfWork;

    public SchemaLoader(IUnitOfWork unitOfWork) =>
        _unitOfWork = Guards.NotDefault(unitOfWork, nameof(unitOfWork));

    public async Task<SchemaInfo> ResolveSchemaAsync(
        JsonDocument schemaSource,
        CancellationToken cancellationToken = default)
    {
        Guards.NotDefault(schemaSource, nameof(schemaSource));

        var tracker = new SchemaDetailTracker();
        var resolved = await JsonSchema.FromJsonAsync(schemaSource.RootElement.ToString(), ".",
            s => Resolve(SchemaDetailTracker.RootSchemaName, s, tracker), cancellationToken);
        tracker.SetSchema(SchemaDetailTracker.RootSchemaName, schemaSource, resolved);
        var info = new SchemaInfo(tracker.Root, tracker.References);
        return info;
    }

    private static List<string> GetSchemaReferences(JToken doc) =>
        doc.SelectTokens("$..['$ref']").Select(t => t.Value<string>()).ToList()!;

    private JsonReferenceResolver Resolve(string schemaName, JsonSchema schema, SchemaDetailTracker tracker)
    {
        var references = GetSchemaReferences(JObject.Parse(schema.ToJson()));
        var schemaResolver = new JsonSchemaResolver(schema, new JsonSchemaGeneratorSettings());
        var referenceResolver = new JsonReferenceResolver(schemaResolver);
        foreach (var referenceSchemaName in references)
        {
            if (!tracker.Exists(referenceSchemaName))
            {
                // hack - .RESULT
                var referenceJson = _unitOfWork.GlobalSchemas
                    .FindAsync(new GlobalSchemaName(referenceSchemaName), CancellationToken.None)
                    .Result.SingleOrDefault();
                if (referenceJson == null)
                    throw new InvalidOperationException("GlobalSchema doesn't exist: " + referenceSchemaName);
                var schemaRef = referenceJson.Schema.ToJsonNetJson();
                var referenceSchema = JsonSchema
                    .FromJsonAsync(schemaRef.ToString(), ".", s => Resolve(referenceSchemaName, s, tracker),
                        CancellationToken.None).Result;
                tracker.SetSchema(referenceSchemaName, referenceJson.Schema, referenceSchema);
            }

            // add to the schema
            referenceResolver.AddDocumentReference(referenceSchemaName,
                tracker.GetSchema(referenceSchemaName).ResolvedSchema);

            // add to the tracker
            tracker.AddReference(schemaName, referenceSchemaName);
        }

        return referenceResolver;
    }
}
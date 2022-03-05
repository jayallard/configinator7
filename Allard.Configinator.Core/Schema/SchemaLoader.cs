using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications.Schema;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Allard.Configinator.Core.Schema;

public class SchemaLoader
{
    private readonly IUnitOfWork _unitOfWork;

    public SchemaLoader(IUnitOfWork unitOfWork)
    {
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task<SchemaInfo> ResolveSchemaAsync(SchemaName schemaName)
    {
        var schema = await _unitOfWork.Schemas.FindOneAsync(new SchemaNameIs(schemaName));
        return await ResolveSchemaAsync(schema.SchemaName, schema.Schema);
    }

    public async Task<SchemaInfo> ResolveSchemaAsync(
        SchemaName schemaName,
        JsonDocument schemaSource,
        CancellationToken cancellationToken = default)
    {
        Guards.HasValue(schemaSource, nameof(schemaSource));

        var tracker = new SchemaDetailTracker(schemaName);
        var resolved = await JsonSchema.FromJsonAsync(schemaSource.RootElement.ToString(), ".",
            s => Resolve(tracker.RootSchemaName, s, tracker), cancellationToken);
        tracker.SetSchema(tracker.RootSchemaName, schemaSource, resolved);
        var info = new SchemaInfo(tracker.Root, tracker.References);
        return info;
    }

    private static List<SchemaName> GetSchemaReferences(JToken doc)
    {
        return doc.SelectTokens("$..['$ref']").Select(t => new SchemaName(t.Value<string>())).ToList()!;
    }

    private JsonReferenceResolver Resolve(SchemaName schemaName, JsonSchema schema,
        SchemaDetailTracker tracker)
    {
        var references = GetSchemaReferences(JObject.Parse(schema.ToJson()));
        var schemaResolver = new JsonSchemaResolver(schema, new JsonSchemaGeneratorSettings());
        var referenceResolver = new JsonReferenceResolver(schemaResolver);
        foreach (var referenceSchemaName in references)
        {
            if (!tracker.Exists(referenceSchemaName))
            {
                // hack - .RESULT
                var referenceJson = _unitOfWork
                    .Schemas
                    .FindOneAsync(SchemaNameIs.Is(referenceSchemaName), CancellationToken.None).Result;

                // sections can only use it's own schemas, and global schemas
                //EnsureSchemaCanBeUsed(sectionId, referenceJson);

                var schemaRef = referenceJson.Schema.ToJsonNetJson();
                var referenceSchema = JsonSchema
                    .FromJsonAsync(schemaRef.ToString(), ".", s => Resolve(referenceSchemaName, s, tracker),
                        CancellationToken.None).Result;
                tracker.SetSchema(
                    referenceJson.SchemaName,
                    referenceJson.Schema,
                    referenceSchema);
            }

            // add to the schema
            referenceResolver.AddDocumentReference(referenceSchemaName.FullName,
                tracker.GetSchema(referenceSchemaName).ResolvedSchema);

            // add to the tracker
            tracker.AddReference(schemaName, referenceSchemaName);
        }

        return referenceResolver;
    }
}
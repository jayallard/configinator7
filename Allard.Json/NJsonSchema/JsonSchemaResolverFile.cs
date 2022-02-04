using System.Xml.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Allard.Json.NJsonSchema;

public class JsonSchemaResolverFile : IJsonSchemaResolver
{
    private readonly Func<SchemaId, FileInfo?> _getFileForId;

    public JsonSchemaResolverFile(Func<SchemaId, FileInfo?> getFileForId)
    {
        _getFileForId = getFileForId;
    }

    public async Task<JObject> GetJsonSchema(SchemaId id, CancellationToken cancellationToken = default)
    {
        var file = _getFileForId(id);
        if (file == null) throw new InvalidOperationException("Unable to load schema: " + id.Id);
        using var fileReader = File.OpenText(file.FullName);
        using var jsonReader = new JsonTextReader(fileReader);
        return (JObject)await JToken.ReadFromAsync(jsonReader, cancellationToken);
    }
}

public record SchemaId(string Id);
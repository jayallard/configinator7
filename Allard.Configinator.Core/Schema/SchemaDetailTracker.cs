using System.Collections.ObjectModel;
using System.Text.Json;
using NJsonSchema;

namespace Allard.Configinator.Core.Schema;

/// <summary>
/// As as schema is resolved, it's references need to be resolved.
/// Schemas refer to other schemas, endlessly recursively, etc.
/// </summary>
public class SchemaDetailTracker
{
    public const string RootSchemaName = "/";
    private readonly Dictionary<string, SchemaDetail> _schemas = new(StringComparer.OrdinalIgnoreCase);

    public ReadOnlyCollection<SchemaDetail> References => _schemas.Values.ToList().AsReadOnly();

    public SchemaDetail Root => GetOrCreate(RootSchemaName);

    public SchemaDetail GetSchema(string name) => _schemas[name];

    public bool Exists(string name) => _schemas.ContainsKey(name);
    
    private SchemaDetail GetOrCreate(string name)
    {
        if (!_schemas.ContainsKey(name))
        {
            _schemas[name] = new SchemaDetail(name);
        }

        return _schemas[name];
    }

    public void SetSchema(string name, JsonDocument schemaSource, JsonSchema resolved)
    {
        var s = GetOrCreate(name);
        if (s.ResolvedSchema != null) throw new InvalidOperationException("bug");
        s.ResolvedSchema = resolved;
        s.SchemaSource = schemaSource;
    }

    public void AddReference(string name, string refersTo)
    {
        GetOrCreate(name).AddRefersTo(refersTo);
        GetOrCreate(refersTo).AddReferencedBy(name);
    }
}

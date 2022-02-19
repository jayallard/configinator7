using System.Collections.ObjectModel;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using NJsonSchema;

namespace Allard.Configinator.Core.Schema;

/// <summary>
///     As as schema is resolved, it's references need to be resolved.
///     Schemas refer to other schemas, endlessly recursively, etc.
/// </summary>
public class SchemaDetailTracker
{
    private readonly Dictionary<SchemaName, SchemaDetail> _schemas = new();

    public SchemaDetailTracker(SchemaName rootSchemaName)
    {
        RootSchemaName = rootSchemaName;
    }

    public SchemaName RootSchemaName { get; }

    public ReadOnlyCollection<SchemaDetail> AllSchemas => _schemas.Values.ToList().AsReadOnly();

    public ReadOnlyCollection<SchemaDetail> References =>
        _schemas.Values.Where(v => RootSchemaName != v.SchemaName).ToList().AsReadOnly();


    public SchemaDetail Root => GetOrCreate(RootSchemaName);

    public SchemaDetail GetSchema(SchemaName name)
    {
        return _schemas[name];
    }

    public bool Exists(SchemaName name)
    {
        return _schemas.ContainsKey(name);
    }

    private SchemaDetail GetOrCreate(SchemaName name)
    {
        if (!_schemas.ContainsKey(name)) _schemas[name] = new SchemaDetail(name);

        return _schemas[name];
    }

    public void SetSchema(SchemaName schemaName, JsonDocument schemaSource, JsonSchema resolved)
    {
        var s = GetOrCreate(schemaName);
        if (s.ResolvedSchema != null) throw new InvalidOperationException("bug");
        s.ResolvedSchema = resolved;
        s.SchemaSource = schemaSource;
    }

    public void AddReference(SchemaName name, SchemaName refersTo)
    {
        GetOrCreate(name).AddRefersTo(refersTo);
        GetOrCreate(refersTo).AddReferencedBy(name);
    }
}
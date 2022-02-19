using System.Collections.ObjectModel;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using NJsonSchema;

namespace Allard.Configinator.Core.Schema;

public class SchemaDetail
{
    private readonly List<SchemaName> _referencedBy = new();
    private readonly List<SchemaName> _refersTo = new();

    /// <summary>
    ///     Initialize a new instance of the SchemaDetail class.
    /// </summary>
    /// <param name="schemaName"></param>
    public SchemaDetail(SchemaName schemaName)
    {
        SchemaName = schemaName;
    }

    /// <summary>
    ///     Gets the schema.
    /// </summary>
    public JsonSchema ResolvedSchema { get; internal set; }

    public JsonDocument SchemaSource { get; internal set; }

    /// <summary>
    ///     Gets the name of the schema.
    /// </summary>
    public SchemaName SchemaName { get; }

    /// <summary>
    ///     All schemas that use this schema.
    /// </summary>
    public ReadOnlyCollection<SchemaName> ReferencedBy => _referencedBy.AsReadOnly();

    /// <summary>
    ///     All schemas that this schema refers to.
    /// </summary>
    public ReadOnlyCollection<SchemaName> RefersTo => _refersTo.AsReadOnly();

    /// <summary>
    ///     Indicate a schema that this schema refers to.
    /// </summary>
    /// <param name="refersTo"></param>
    internal void AddRefersTo(SchemaName name)
    {
        _refersTo.Add(name);
    }

    /// <summary>
    ///     Indicate a schema that this one refers to.
    /// </summary>
    /// <param name="referencedBy"></param>
    internal void AddReferencedBy(SchemaName referencedBy)
    {
        _referencedBy.Add(referencedBy);
    }
}
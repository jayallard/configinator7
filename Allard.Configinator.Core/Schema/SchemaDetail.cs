using System.Collections.ObjectModel;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using NJsonSchema;

namespace Allard.Configinator.Core.Schema;

public class SchemaDetail
{
    private readonly List<string> _referencedBy = new();
    private readonly List<string> _refersTo = new();

    /// <summary>
    /// Initialize a new instance of the SchemaDetail class.
    /// </summary>
    /// <param name="name"></param>
    public SchemaDetail(string name) => Name = new SchemaName(name);
    
    /// <summary>
    /// Gets the schema.
    /// </summary>
    public JsonSchema ResolvedSchema { get; internal set; }

    public JsonDocument SchemaSource { get; internal set; }

    /// <summary>
    /// Gets the name of the schema.
    /// </summary>
    public SchemaName Name { get; }

    /// <summary>
    /// All schemas that use this schema.
    /// </summary>
    public ReadOnlyCollection<string> ReferencedBy => _referencedBy.AsReadOnly();

    /// <summary>
    /// All schemas that this schema refers to.
    /// </summary>
    public ReadOnlyCollection<string> RefersTo => _refersTo.AsReadOnly();

    /// <summary>
    /// Indicate a schema that this schema refers to.
    /// </summary>
    /// <param name="refersTo"></param>
    internal void AddRefersTo(string refersTo) => _refersTo.Add(refersTo);

    /// <summary>
    /// Indicate a schema that this one refers to.
    /// </summary>
    /// <param name="referencedBy"></param>
    internal void AddReferencedBy(string referencedBy) => _referencedBy.Add(referencedBy);
}
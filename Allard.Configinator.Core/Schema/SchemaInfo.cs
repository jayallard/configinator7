using System.Collections.ObjectModel;
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Schema;

public record SchemaInfo(SchemaDetail Root, ReadOnlyCollection<SchemaDetail> References)
{
    /// <summary>
    ///     Gets a value indicating whether the schema, or any of the schemas it references,
    ///     have a pre-release version.
    /// </summary>
    public bool IsPreRelease() =>
        Root.SchemaName.Version.IsPrerelease || References.Any(r => r.SchemaName.Version.IsPrerelease);
    
    // TODO: see how RECORDS deal with computed properties for equals/hashcode, etc.
    public IEnumerable<SchemaName> AllNames() => References
        .Select(r => r.SchemaName)
        .Union(new[] {Root.SchemaName})
        .Distinct();
}
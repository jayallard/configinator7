using System.Collections.ObjectModel;

namespace Allard.Configinator.Core.Schema;

public record SchemaInfo(SchemaDetail Root, ReadOnlyCollection<SchemaDetail> References)
{
    /// <summary>
    /// Gets a value indicating whether the schema, or any of the schemas it references,
    /// have a pre-release version.
    /// </summary>
    public bool IsPreRelease => Root.Name.Version.IsPrerelease || References.Any(r => r.Name.Version.IsPrerelease);
};
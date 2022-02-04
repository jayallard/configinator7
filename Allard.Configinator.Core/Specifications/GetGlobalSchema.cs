using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Specifications;

public class GetGlobalSchema : ISpecification<GlobalSchemaAggregate>
{
    public GetGlobalSchema(string name, SemanticVersion version)
    {
        Name = name;
        Version = version;
    }

    public string Name { get; }
    public SemanticVersion Version { get; }

    public bool IsSatisfied(GlobalSchemaAggregate obj) =>
        obj.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)
        && obj.Version == Version;
}
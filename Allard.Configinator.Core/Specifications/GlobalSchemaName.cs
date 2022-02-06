using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Specifications;

public class GlobalSchemaName : ISpecification<GlobalSchemaAggregate>
{
    public GlobalSchemaName(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool IsSatisfied(GlobalSchemaAggregate obj) =>
        obj.Name.Equals(Name, StringComparison.OrdinalIgnoreCase);
}
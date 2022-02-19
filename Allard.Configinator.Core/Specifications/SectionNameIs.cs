using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class SectionNameIs : ISpecification<SectionAggregate>
{
    public SectionNameIs(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool IsSatisfied(SectionAggregate obj)
    {
        return obj.SectionName.Equals(Name, StringComparison.OrdinalIgnoreCase);
    }
}
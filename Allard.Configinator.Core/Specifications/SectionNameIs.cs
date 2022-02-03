using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class SectionNameIs : ISpecification<SectionAggregate>
{
    public string Name { get; }
    public SectionNameIs(string name)
    {
        Name = name;
    }

    public bool IsSatisfied(SectionAggregate obj)
    {
        return obj.SectionName.Equals(Name, StringComparison.OrdinalIgnoreCase);
    }
}
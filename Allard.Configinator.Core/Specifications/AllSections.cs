using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class AllSections : ISpecification<SectionAggregate>
{
    public bool IsSatisfied(SectionAggregate obj)
    {
        return true;
    }
}
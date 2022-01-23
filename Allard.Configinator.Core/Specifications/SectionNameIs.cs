using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class SectionNameIs : ISpecification<SectionEntity>
{
    public string Name { get; }
    public SectionNameIs(string name)
    {
        Name = name;
    }

    public bool IsSatisfied(SectionEntity obj)
    {
        return obj.SectionName.Equals(Name, StringComparison.OrdinalIgnoreCase);
    }
}
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class SectionByName : ISpecification<SectionEntity>
{
    public string Name { get; }
    public SectionByName(string name)
    {
        Name = name;
    }

    public bool IsSatisfied(SectionEntity obj)
    {
        return obj.SectionName.Equals(Name, StringComparison.OrdinalIgnoreCase);
    }
}
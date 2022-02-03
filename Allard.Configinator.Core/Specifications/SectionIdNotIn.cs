using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class SectionIdNotIn : ISpecification<SectionAggregate>
{
    private readonly ISet<long> _except;

    public SectionIdNotIn(IEnumerable<SectionId> ids)
    {
        _except = ids.Select(id => id.Id).ToHashSet();
    }

    public bool IsSatisfied(SectionAggregate obj) =>
        !_except.Contains(obj.Id.Id);
}
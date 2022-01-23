using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class SectionIdNotIn : ISpecification<SectionEntity>
{
    private readonly ISet<long> _except;

    public SectionIdNotIn(IEnumerable<SectionId> ids)
    {
        _except = ids.Select(id => id.Id).ToHashSet();
    }

    public bool IsSatisfied(SectionEntity obj) =>
        !_except.Contains(obj.Id.Id);
}
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class IdNotIn : ISpecification<IAggregate>
{
    private readonly HashSet<long> _ids;

    public IdNotIn(IEnumerable<long> ids)
    {
        _ids = ids.ToHashSet();
    }

    public bool IsSatisfied(IAggregate obj)
    {
        return !_ids.Contains(obj.EntityId);
    }
}
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class IdIsIn : ISpecification<IEntity>
{
    public IdIsIn(IEnumerable<long> ids)
    {
        Ids = ids.ToHashSet();
    }

    public ISet<long> Ids { get; }

    public bool IsSatisfied(IEntity obj)
    {
        return Ids.Contains(obj.EntityId);
    }
}
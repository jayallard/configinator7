using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class IdIsIn : ISpecification<IEntity>
{
    public ISet<long> Ids { get; }

    public IdIsIn(IEnumerable<long> ids) => Ids = ids.ToHashSet();
    public bool IsSatisfied(IEntity obj) => Ids.Contains(obj.EntityId);
}
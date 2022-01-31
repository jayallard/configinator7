using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class All : ISpecification<IEntity>
{
    public bool IsSatisfied(IEntity obj) => true;
}
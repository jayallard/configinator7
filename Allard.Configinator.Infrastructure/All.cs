using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class All : ISpecification<IEntity>
{
    public bool IsSatisfied(IEntity obj) => true;
}
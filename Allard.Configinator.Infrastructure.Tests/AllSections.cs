using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Tests;

public class AllSections : ISpecification<SectionEntity>
{
    public bool IsSatisfied(SectionEntity obj) => true;
}
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class OrganizationPathIs : ISpecification<SectionAggregate>
{
    public string OrganizationPath { get; }
    public OrganizationPathIs(string organizationPath)
    {
        OrganizationPath = organizationPath;
    }

    public bool IsSatisfied(SectionAggregate obj) =>
        obj.OrganizationPath.Equals(OrganizationPath, StringComparison.OrdinalIgnoreCase);
}
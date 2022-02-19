using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class VariableSetNameIs : ISpecification<VariableSetAggregate>
{
    public VariableSetNameIs(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool IsSatisfied(VariableSetAggregate obj)
    {
        return obj.VariableSetName.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
    }
}
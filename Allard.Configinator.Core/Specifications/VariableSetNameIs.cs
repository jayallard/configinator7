using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class VariableSetNameIs : ISpecification<VariableSetAggregate>
{
    public string Name { get; }

    public VariableSetNameIs(string name) => Name = name;

    public bool IsSatisfied(VariableSetAggregate obj) =>
        obj.VariableSetName.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
}
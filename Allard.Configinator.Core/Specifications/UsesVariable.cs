using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

/// <summary>
/// Find all sections which have a release that
/// uses the given variable.
/// </summary>
public class UsesVariable : ISpecification<SectionAggregate>
{
    public UsesVariable(string variableSetName, string variableName)
    {
        VariableSetName = variableSetName;
        VariableName = variableName;
    }

    public string VariableSetName { get; }
    public string VariableName { get; }

    public bool IsSatisfied(SectionAggregate obj) => true;
    // TODO: fix
    // any environment that has any release that's using the variable
    // obj.Environments.Any(
    //     e => e.Releases.Any(r =>
    //         string.Equals(VariableSetName, r.VariableSet?.VariableSetName, StringComparison.OrdinalIgnoreCase) &&
    //         r.VariablesInUse.Contains(TokenName)));
}
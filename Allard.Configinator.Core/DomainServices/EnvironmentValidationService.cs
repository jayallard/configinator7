using NJsonSchema;

namespace Allard.Configinator.Core.DomainServices;

public class EnvironmentValidationService
{
    private readonly EnvironmentRules _rules;

    public EnvironmentValidationService(EnvironmentRules rules)
    {
        // todo: unique environment types, no duplicate types,
        // no duplicate environments across environment types
        _rules = rules;
    }

    /// <summary>
    /// Gets the list of environment types.
    /// </summary>
    public ISet<string> EnvironmentTypeNames => _rules
        .EnvironmentTypes
        .Select(e => e.EnvironmentTypeName)
        .ToHashSet();

    /// <summary>
    /// Returns a list of environment types and environments.
    /// </summary>
    public List<(string EnvironmentType, string EnvironmentName)> EnvironmentNames
    {
        get
        {
            var x = _rules.EnvironmentTypes
                .SelectMany(et => et.AllowedEnvironments
                    .Select(e => (et.EnvironmentTypeName, e)));
            return x.ToList();
        }
    }

    /// <summary>
    /// Returns true if the environment name is valid for the environment type.
    /// </summary>
    /// <param name="environmentName"></param>
    /// <returns></returns>
    public bool IsValidEnvironmentName(string environmentName) =>
        _rules.EnvironmentTypes.Any(et =>
            et.AllowedEnvironments.Contains(environmentName, StringComparer.OrdinalIgnoreCase));

    public string GetEnvironmentType(string environmentName) => EnvironmentNames
        .Single(e => e.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
        .EnvironmentType;
}
using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace Allard.Configinator.Core.DomainServices;

public class EnvironmentValidationService
{
    private readonly EnvironmentRules _rules;

    public EnvironmentValidationService(EnvironmentRules rules)
    {
        // todo: unique environment types, no duplicate types,
        // no duplicate environments across environment types
        _rules = Guards.HasValue(rules, nameof(rules));
    }

    /// <summary>
    ///     Gets the list of environment types.
    /// </summary>
    public ISet<string> EnvironmentTypeNames => _rules
        .EnvironmentTypes
        .Select(e => e.Name)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Returns a list of environment types and environments.
    /// </summary>
    public List<(string EnvironmentType, string EnvironmentName)> EnvironmentNames
    {
        get
        {
            var x = _rules.EnvironmentTypes
                .SelectMany(et => et.AllowedEnvironments
                    .Select(e => (EnvironmentTypeName: et.Name, e)));
            return x.ToList();
        }
    }

    /// <summary>
    ///     Returns true if the environment name is valid for the environment type.
    /// </summary>
    /// <param name="environmentName"></param>
    /// <returns></returns>
    public bool IsValidEnvironmentName(string environmentName)
    {
        return _rules.EnvironmentTypes.Any(et =>
            et.AllowedEnvironments.Contains(environmentName, StringComparer.OrdinalIgnoreCase));
    }

    public bool IsValidEnvironmentType(string environmentType)
    {
        return _rules
            .EnvironmentTypes
            .Any(et => et.Name.Equals(environmentType, StringComparison.OrdinalIgnoreCase));
    }

    public string GetEnvironmentType(string environmentName)
    {
        return EnvironmentNames
            .Single(e => e.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
            .EnvironmentType;
    }

    // TODO: hack
    public string GetFirstEnvironmentType() => "development";


    // TODO: hack
    public static bool IsPreReleaseAllowed(string environmentType)
    {
        return environmentType.Equals("development", StringComparison.OrdinalIgnoreCase);
    }

    // TODO: hack
    public string? GetNextSectionEnvironmentType(IEnumerable<string> environmentTypes)
    {
        var types = environmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!types.Contains("staging")) return "staging";
        if (!types.Contains("production")) return "production";
        return null;
    }

    // hack
    public string? GetNextSchemaEnvironmentType(IEnumerable<string> assignedEnvironmentTypes, SemanticVersion version)
    {
        if (version.IsPrerelease) return null;
        var types = assignedEnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!types.Contains("staging")) return "staging";
        if (!types.Contains("production")) return "production";
        return null;
    }

    public bool CanPromoteSectionTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType)
    {
        return CanPromoteTo(assignedEnvironmentTypes, targetEnvironmentType);
    }

    // hack
    public bool CanPromoteSchemaTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType,
        SchemaName schemaName)
    {
        return !schemaName.Version.IsPrerelease && CanPromoteTo(assignedEnvironmentTypes, targetEnvironmentType);
    }

    private static bool CanPromoteTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType)
    {
        var types = assignedEnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return targetEnvironmentType.ToLower() switch
        {
            "staging" => !types.Contains("staging"),
            "production" => types.Contains("staging") && !types.Contains("production"),
            _ => false
        };
    }
}
using System.Collections.Immutable;
using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace Allard.Configinator.Core.DomainServices;

/// <summary>
/// Environment type names are unique.
/// Environment names are unique across all environment types.
/// </summary>
public class EnvironmentDomainService
{
    private readonly Dictionary<string, EnvironmentType> _byEnvironmentType;
    private readonly Dictionary<string, EnvironmentType> _byEnvironment;
    private readonly List<string> _promotionOrder;

    /// <summary>
    /// Gets all of the environment names.
    /// </summary>
    public ImmutableHashSet<string> EnvironmentNames { get; }

    /// <summary>
    /// Gets all of the environment type objects.
    /// </summary>
    public IEnumerable<EnvironmentType> EnvironmentTypes => _byEnvironmentType.Values.Select(e => e.Clone());

    /// <summary>
    /// Initializes a new instance of the EnvironmentDomainService class.
    /// </summary>
    /// <param name="rules"></param>
    public EnvironmentDomainService(EnvironmentRules rules)
    {
        Guards.HasValue(rules, nameof(rules));
        rules.EnsureIsValidRuleSet();
        
        // store the environment types by environment type
        _byEnvironmentType =
            rules.EnvironmentTypes.ToDictionary(et => et.EnvironmentTypeName, et => et,
                StringComparer.OrdinalIgnoreCase);
        
        // store the environment types by environment name
        _byEnvironment = rules
            .EnvironmentTypes
            .SelectMany(et => et.AllowedEnvironments.Select(e => new {Environment = e, EnvironmentType = et}))
            .ToDictionary(e => e.Environment, e => e.EnvironmentType, StringComparer.OrdinalIgnoreCase);
        
        _promotionOrder = rules.NamesInPromotionOrder();
    }

    /// <summary>
    ///     Returns true if the environment name is valid for the environment type.
    /// </summary>
    /// <param name="environmentName"></param>
    /// <returns></returns>
    public bool EnvironmentExists(string environmentName) => _byEnvironment.ContainsKey(environmentName);

    /// <summary>
    /// Returns true if the Environment Type exists.
    /// </summary>
    /// <param name="environmentType"></param>
    /// <returns></returns>
    public bool EnvironmentTypeExists(string environmentType) => _byEnvironmentType.ContainsKey(environmentType);

    /// <summary>
    /// Get an environment type by name.
    /// </summary>
    /// <param name="environmentTypeName"></param>
    /// <returns></returns>
    public EnvironmentType GetEnvironmentType(string environmentTypeName) => _byEnvironmentType[environmentTypeName].Clone();

    /// <summary>
    /// Get the environment type for an enviornment.
    /// </summary>
    /// <param name="environmentName"></param>
    /// <returns></returns>
    public EnvironmentType GetEnvironmentTypeForEnvironment(string environmentName) => _byEnvironment[environmentName].Clone();

    /// <summary>
    /// Gets the first environment type in the promotion order.
    /// </summary>
    /// <returns></returns>
    public EnvironmentType GetFirstEnvironmentType() => _byEnvironmentType[_promotionOrder.First()].Clone();

    /// <summary>
    /// Returns the next environment type in the promotion order.
    /// IE: if the promotion order is a -> b -> c -> d -> e
    /// and the list is (a, b, c), this will return d.
    /// </summary>
    /// <param name="environmentTypes"></param>
    /// <returns></returns>
    public EnvironmentType? GetNextEnvironmentType(IEnumerable<string> environmentTypes)
    {
        var highest = GetHighestEnvironmentType(environmentTypes);
        if (highest == null) return null;
        var next = _byEnvironmentType[highest].Next;
        return next == null
            ? null
            : _byEnvironmentType[next].Clone();
    }

    /// <summary>
    /// Give a list of environment types, find the one that is
    /// the highest in the promotion order.
    /// IE: if the promotion order is dev -> staging -> production,
    /// and the parameter value is (dev, staging), this will return staging.
    /// </summary>
    /// <param name="environmentTypes"></param>
    /// <returns></returns>
    private string? GetHighestEnvironmentType(IEnumerable<string> environmentTypes)
    {
        return
            _promotionOrder
                .Join(environmentTypes, l => l, r => r, (l, r) => l)
                .LastOrDefault();
    }

    /// <summary>
    /// Returns the next environment type for a schema.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public string? GetNextSchemaEnvironmentType(IEnumerable<string> assignedEnvironmentTypes, SemanticVersion version)
    {
        // TODO: doesn't belong here.. move this to the appropriate service
        var next = GetNextEnvironmentType(assignedEnvironmentTypes);
        if (next == null) return null;

        // if the schema isn't a prerelease, then return the next environment type.
        // if the schema is a prerelease, then return the next environment type only if it supports prerelease.
        return (version.IsPrerelease && next.SupportsPreRelease) || !version.IsPrerelease
            ? next.EnvironmentTypeName
            : null;
    }

    /// <summary>
    /// Returns true if the section can be prompted to the target environment type.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <returns></returns>
    public void EnsureCanPromoteSectionTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType) =>
        EnsureCanPromoteTo(assignedEnvironmentTypes, targetEnvironmentType);

    /// <summary>
    /// Returns TRUE if the schema can be promoted to the target environment type.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <param name="schemaName"></param>
    /// <returns></returns>
    public void EnsureCanPromoteSchemaTo(
        IEnumerable<string> assignedEnvironmentTypes,
        string targetEnvironmentType,
        SchemaName schemaName)
    {
        if (schemaName.Version.IsPrerelease && !GetEnvironmentType(targetEnvironmentType).SupportsPreRelease)
        {
            throw new InvalidOperationException(
                "The schema can't be promoted because PreRelease schemas aren't supported in the target environment type. Target Environment Type=" +
                targetEnvironmentType);
        }

        EnsureCanPromoteTo(assignedEnvironmentTypes, targetEnvironmentType);
    }

    /// <summary>
    /// Returns true if an item can be promoted to the target environment type.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <returns></returns>
    private void EnsureCanPromoteTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType)
    {
        var next = GetNextEnvironmentType(assignedEnvironmentTypes);
        if (next == null || !targetEnvironmentType.Equals(next.EnvironmentTypeName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The schema can't be promoted because the target environment type is out of order. Target Environment Type=" +
                targetEnvironmentType);
        }
    }
}
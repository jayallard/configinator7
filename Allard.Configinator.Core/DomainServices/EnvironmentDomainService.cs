using System.Collections.Immutable;
using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace Allard.Configinator.Core.DomainServices;

public class EnvironmentDomainService
{
    private readonly Dictionary<string, EnvironmentType> _byEnvironmentType;
    private readonly Dictionary<string, EnvironmentType> _byEnvironment;
    private readonly List<string> _promotionOrder;

    /// <summary>
    /// Gets the first environment type in the promotion order.
    /// </summary>
    public EnvironmentType FirstEnvironmentType => _byEnvironmentType[_promotionOrder.First()].Clone();

    /// <summary>
    /// Gets the last environment in the promotion order.
    /// </summary>
    public EnvironmentType LastEnvironmentType => _byEnvironmentType[_promotionOrder.Last()].Clone();

    /// <summary>
    /// Gets all of the environment names.
    /// </summary>
    public ImmutableHashSet<string> EnvironmentNames { get; }
    
    /// <summary>
    /// Gets all of the environment type names.
    /// </summary>
    public ImmutableHashSet<string> EnvironmentTypeNames { get; }

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
        _byEnvironmentType =
            rules.EnvironmentTypes.ToDictionary(et => et.EnvironmentTypeName, et => et,
                StringComparer.OrdinalIgnoreCase);
        _byEnvironment = rules
            .EnvironmentTypes
            .SelectMany(et => et.AllowedEnvironments.Select(e => new {Environment = e, EnvironmentType = et}))
            .ToDictionary(e => e.Environment, e => e.EnvironmentType, StringComparer.OrdinalIgnoreCase);
        EnvironmentNames = _byEnvironment.Keys.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        EnvironmentTypeNames = _byEnvironmentType.Values.SelectMany(r => r.AllowedEnvironments)
            .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

        _promotionOrder = rules.NamesInPromotionOrder();
    }

    /// <summary>
    ///     Returns true if the environment name is valid for the environment type.
    /// </summary>
    /// <param name="environmentName"></param>
    /// <returns></returns>
    public bool EnvironmentExists(string environmentName) => EnvironmentNames.Contains(environmentName);

    /// <summary>
    /// Returns true if the Environment Type exists.
    /// </summary>
    /// <param name="environmentType"></param>
    /// <returns></returns>
    public bool EnvironmentTypeExists(string environmentType) => EnvironmentTypeNames.Contains(environmentType);

    /// <summary>
    /// Gets the environment type that the environment belongs to.
    /// </summary>
    /// <param name="environmentName"></param>
    /// <returns></returns>
    public EnvironmentType GetEnvironmentType(string environmentName) => _byEnvironment[environmentName].Clone();

    /// <summary>
    /// Gets the first environment type in the promotion order.
    /// </summary>
    /// <returns></returns>
    public EnvironmentType GetFirstEnvironmentType() => FirstEnvironmentType;

    public bool IsPreReleaseAllowed(string environmentType) =>
        _byEnvironmentType[environmentType].SupportsPreRelease;

    /// <summary>
    /// Returns the next environment type in the promotion order.
    /// IE: if the promotion order is a -> b -> c -> d -> e
    /// and the list is (a, b, c), this will return d.
    /// </summary>
    /// <param name="environmentTypes"></param>
    /// <returns></returns>
    public string? GetNextEnvironmentType(IEnumerable<string> environmentTypes)
    {
        var highest = GetHighestEnvironmentType(environmentTypes);
        return highest == null 
            ? null 
            : _byEnvironmentType[highest].Next;
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
        var highest = _promotionOrder
            .Join(environmentTypes, l => l, r => r, (l, r) => l)
            .ToArray();
        return highest.LastOrDefault();
    }

    /// <summary>
    /// Returns the next environment type for a schema.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public string? GetNextSchemaEnvironmentType(IEnumerable<string> assignedEnvironmentTypes, SemanticVersion version)
    {
        var next = GetNextEnvironmentType(assignedEnvironmentTypes);
        if (next == null) return null;

        // if the schema isn't a prerelease, then return the next environment type.
        // if the schema is a prerelease, then return the next environment type only if it supports prerelease.
        return (version.IsPrerelease && _byEnvironmentType[next].SupportsPreRelease) || !version.IsPrerelease
            ? next
            : null;
    }

    /// <summary>
    /// Returns true if the section can be prompted to the target environment type.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <returns></returns>
    public bool CanPromoteSectionTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType) =>
        CanPromoteTo(assignedEnvironmentTypes, targetEnvironmentType);

    /// <summary>
    /// Returns TRUE if the schema can be promoted to the target environment type.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <param name="schemaName"></param>
    /// <returns></returns>
    public bool CanPromoteSchemaTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType,
        SchemaName schemaName) =>
        !schemaName.Version.IsPrerelease && CanPromoteTo(assignedEnvironmentTypes, targetEnvironmentType);

    
    /// <summary>
    /// Returns true if an item can be promoted to the target environment type.
    /// </summary>
    /// <param name="assignedEnvironmentTypes"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <returns></returns>
    private bool CanPromoteTo(IEnumerable<string> assignedEnvironmentTypes, string targetEnvironmentType)
    {
        var next = GetNextEnvironmentType(assignedEnvironmentTypes);
        return next != null && targetEnvironmentType.Equals(next, StringComparison.OrdinalIgnoreCase);
    }
}

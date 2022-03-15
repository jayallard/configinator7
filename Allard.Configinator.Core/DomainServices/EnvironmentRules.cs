namespace Allard.Configinator.Core.DomainServices;

// TODO: minimal implementation; rules are hardcoded elsewhere. mocking it up.

/// <summary>
/// </summary>
public class EnvironmentRules
{
    public void EnsureIsValidRuleSet()
    {
        EnsureUnique(EnvironmentTypes
            .Select(et => et.EnvironmentTypeName), "Environment Types");
        EnsureUnique(EnvironmentTypes
            .SelectMany(et => et.AllowedEnvironments), "Environment Names");
        EnsureUnique(EnvironmentTypes
            .Select(et => et.Next)
            .Where(et => et is not null), "Next Environment Type");

        // every environment type links to the next environment type, except for the last one.
        // dev -> staging -> production - production.next = null
        // dev. dev.next = null
        if (EnvironmentTypes.Count > 1)
        {
            // multiple environment types
            var nullCount = EnvironmentTypes.Count(et => et.Next is null);
            if (nullCount > 1)
            {
                throw new InvalidOperationException(
                    "Every environment type must have a NEXT value, except for the last one.");
            }
        }
        else if (EnvironmentTypes.Single().Next != null)
        {
            throw new InvalidOperationException("There is only one environment type. It cannot have a NET value.");
        }

        // make sure every NEXT is valid, and each is only used once.
        // valid: dev -> staging -> production
        // invalid: dev -> dev -> staging
        // invalid: dev -> staging -> dev
        var environmentTypes = EnvironmentTypes.Select(r => r.EnvironmentTypeName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var nextEnvironmentTypes = EnvironmentTypes
            .Select(r => r.Next)
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // find all the NEXT that don't exist.
        var invalidNextEnvironmentTypes = nextEnvironmentTypes.Where(n => !environmentTypes.Contains(n!)).ToArray();
        if (invalidNextEnvironmentTypes.Any())
        {
            var error = string.Join(",", invalidNextEnvironmentTypes);
            throw new InvalidOperationException("Invalid Next Environment Types: " + error);
        }

        // make sure that once PRE RELEASE isn't allowed, it isn't subsequently allowed again.
        // IE: if allowed in dev, but not staging, it can't be allowed in production.
        var inOrder = NamesInPromotionOrder();
        var isAllowed = EnvironmentTypes.Single(et => et.EnvironmentTypeName.Equals(inOrder[0])).SupportsPreRelease;
        foreach (var et in inOrder.Skip(1))
        {
            var next = EnvironmentTypes.Single(et => et.EnvironmentTypeName.Equals(inOrder[0]));
            switch (next.SupportsPreRelease)
            {
                case false:
                    isAllowed = false;
                    continue;
                case true when !isAllowed:
                    throw new InvalidOperationException(
                        "The environment type can't support PreRelease because the prior environment type does not: " +
                        next.EnvironmentTypeName);
            }
        }
    }

    private static void EnsureUnique<T>(IEnumerable<T> items, string itemType)
    {
        // items of type itemType must be unique.
        var duplicateItems = items
            .GroupBy(i => i)
            .Where(et => et.Count() > 1)
            .ToArray();

        if (!duplicateItems.Any()) return;
        var duplicates = string.Join(", ", duplicateItems.Select(d => d.Key));
        throw new InvalidOperationException($"Duplicate {itemType}: " + duplicates);
    }

    internal List<string> NamesInPromotionOrder()
    {
        var inOrder = new List<string>();
        var current = EnvironmentTypes.Single(r => r.Next is null);
        inOrder.Add(current.EnvironmentTypeName);
        while (true)
        {
            var previous = EnvironmentTypes.SingleOrDefault(
                et => et.Next is not null
                      && et.Next.Equals(current.EnvironmentTypeName, StringComparison.OrdinalIgnoreCase));
            if (previous == null) break;
            inOrder.Add(previous.EnvironmentTypeName);
            current = previous;
        }

        inOrder.Reverse();
        return inOrder;
    }

    /// <summary>
    ///     Gets or sets the environment type / environment configuration.
    /// </summary>
    public List<EnvironmentType> EnvironmentTypes { get; set; }

    public EnvironmentRules Clone()
    {
        return new EnvironmentRules
        {
            EnvironmentTypes = EnvironmentTypes.Select(et => et.Clone()).ToList()
        };
    }
}

public class EnvironmentType
{
    /// <summary>
    ///     Gets or sets the name of th environment type.
    /// </summary>
    public string EnvironmentTypeName { get; set; }

    /// <summary>
    ///     Gets or sets the names of the environments for
    ///     this environment type.
    /// </summary>
    public string[] AllowedEnvironments { get; set; }

    public string? Next { get; set; }

    public bool SupportsPreRelease { get; set; }

    public EnvironmentType Clone() => new()
    {
        EnvironmentTypeName = EnvironmentTypeName,
        AllowedEnvironments = AllowedEnvironments,
        Next = Next,
        SupportsPreRelease = SupportsPreRelease
    };
}
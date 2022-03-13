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
        if (EnvironmentTypes.Count > 1)
        {
            var nullCount = EnvironmentTypes.Count(et => et.Next is null);
            if (nullCount > 1)
            {
                throw new InvalidOperationException(
                    "Every environment type must have a NEXT value, except for the last one.");
            }
        }
        else
        {
            // only on environment type. makes ure it doesn't have a next.
            if (EnvironmentTypes.Single().Next != null)
            {
                throw new InvalidOperationException("There is only one environment type. It cannot have a NET value.");
            }
        }

        // make sure every NEXT is valid, and each is only used once.
        // valid: dev -> staging -> production
        // invalid: dev -> dev -> staging
        // invalid: dev -> staging -> dev
        var environmentTypes = EnvironmentTypes.Select(r => r.EnvironmentTypeName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var nextEnvironmentTypes = EnvironmentTypes
            .Select(r => r.Next)
            .Where(n => n is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // find all the NEXT that don't exist.
        var invalidNextEnvironmentTypes = nextEnvironmentTypes.Where(n => !environmentTypes.Contains(n!)).ToArray();
        if (!invalidNextEnvironmentTypes.Any()) return;
        var error = string.Join(",", invalidNextEnvironmentTypes);
        throw new InvalidOperationException("Invalid Next Environment Types: " + error);
    }

    private static void EnsureUnique<T>(IEnumerable<T> items, string itemType)
    {
        // environment types must be unique
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
            EnvironmentTypes =
                EnvironmentTypes is null
                    ? null
                    : EnvironmentTypes.Select(et => et.Clone()).ToList()
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

    public EnvironmentType Clone() => new()
    {
        EnvironmentTypeName = EnvironmentTypeName,
        AllowedEnvironments = AllowedEnvironments,
        Next = Next
    };
}
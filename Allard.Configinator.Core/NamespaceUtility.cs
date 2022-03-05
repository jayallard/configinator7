namespace Allard.Configinator.Core;

public static class NamespaceUtility
{
    public static bool IsSelfOrAscendant(string ascendant, string descendant)
    {
        // ascendant:   /a/b/c/d
        // descendant:  /a/b/c/d/e
        ascendant = NormalizeNamespace(ascendant);
        if (ascendant != "/") ascendant += "/";
        descendant = NormalizeNamespace(descendant) + '/';
        return descendant.StartsWith(ascendant, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeNamespace(string? @namespace)
    {
        if (@namespace == null) return null;
        if (!@namespace.StartsWith('/')) @namespace = '/' + @namespace;
        return @namespace.Length > 1 ? @namespace.TrimEnd('/') : @namespace;
    }
}
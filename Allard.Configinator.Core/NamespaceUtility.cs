namespace Allard.Configinator.Core;

public static class NamespaceUtility
{
    public static bool IsSelfOrAscendant(string ascendant, string descendant)
    {
        // ascendant:   /a/b/c/d
        // descendant:  /a/b/c/d/e
        if (ascendant != "/") ascendant += "/";
        if (descendant != "/") descendant += "/";
        return descendant.StartsWith(ascendant, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeNamespace(string? @namespace)
    {
        // shouldn't need this anymore; namespace is validated on creation.
        return @namespace;
        // if (@namespace == null) return null;
        // if (!@namespace.StartsWith('/')) @namespace = '/' + @namespace;
        // return @namespace.Length > 1 ? @namespace.TrimEnd('/') : @namespace;
    }
}
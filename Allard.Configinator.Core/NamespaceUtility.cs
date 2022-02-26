namespace Allard.Configinator.Core;

public static class NamespaceUtility
{
    public static bool IsSelfOrAscendant(string test, string against)
    {
        test = NormalizeNamespace(test);
        against = NormalizeNamespace(against);
        return test.StartsWith(against, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeNamespace(string? @namespace)
    {
        if (@namespace == null) return null;
        if (!@namespace.StartsWith('/')) @namespace = '/' + @namespace;
        return @namespace.TrimEnd('/');
    }
}
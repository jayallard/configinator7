namespace Allard.Json;

public static class VariableSetComposer
{
    public static VariableSetComposed Compose(IEnumerable<VariableSet> variableSets, string variableSetName) =>
        Compose(variableSets)[variableSetName];

    public static Dictionary<string, VariableSetComposed> Compose(IEnumerable<VariableSet> variableSets)
    {
        var originals = variableSets.ToList();
        var sets = originals.ToDictionary(
            v => v.VariableSetName,
            v => new VariableSetComposed(v.Variables, v.VariableSetName),
            StringComparer.OrdinalIgnoreCase);

        var setsWithBase = originals.Where(o => o.BaseVariableSetName is not null);
        foreach (var s in setsWithBase)
        {
            var theBase = sets[s.BaseVariableSetName!];
            var child = sets[s.VariableSetName];
            theBase.AddChild(child);
            child.BaseVariableSet = theBase;
        }

        return sets;
    }

}
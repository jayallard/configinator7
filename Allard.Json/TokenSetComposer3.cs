namespace Allard.Json;

public static class TokenSetComposer3
{
    public static TokenSetComposed3 Compose(IEnumerable<TokenSet> tokenSets, string tokenSetName) =>
        Compose(tokenSets)[tokenSetName];

    public static Dictionary<string, TokenSetComposed3> Compose(IEnumerable<TokenSet> tokenSets)
    {
        var originals = tokenSets.ToList();
        var sets = originals.ToDictionary(
            v => v.TokenSetName,
            v => new TokenSetComposed3(v.Tokens, v.TokenSetName),
            StringComparer.OrdinalIgnoreCase);

        var setsWithBase = originals.Where(o => o.BaseTokenSetName is not null);
        foreach (var s in setsWithBase)
        {
            var theBase = sets[s.BaseTokenSetName!];
            var child = sets[s.TokenSetName];
            theBase.AddChild(child);
            child.BaseTokenSet = theBase;
        }

        return sets;
    }

}
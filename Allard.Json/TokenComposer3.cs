namespace Allard.Json;

public static class TokenComposer3
{
    public static TokenSetComposed3 Compose(IEnumerable<TokenSet> tokenSets, string tokenSetName)
    {
        var originals = tokenSets.ToList();
        var sets = originals.ToDictionary(
            v => v.TokenSetName,
            v => new TokenSetComposed3(v.Tokens, v.TokenSetName),
            StringComparer.OrdinalIgnoreCase);
        
        foreach (var s in originals.Where(o => o.BaseTokenSetName is not null))
        {
            var theBase = sets[s.BaseTokenSetName!];
            var child = sets[s.TokenSetName];
            theBase.AddChild(child);
            child.BaseTokenSet = theBase;
        }

        return sets[tokenSetName];
    }
}
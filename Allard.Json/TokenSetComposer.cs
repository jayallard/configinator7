using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class TokenSetComposer
{
    private readonly Dictionary<string, TokenSet> _tokenSets;

    public TokenSetComposer(IEnumerable<TokenSet> tokenSets)
    {
        _tokenSets = tokenSets.ToDictionary(t => t.TokenSetName, t => t, StringComparer.OrdinalIgnoreCase);
    }

    private TokenSet GetTokenSet(string tokenSetName)
    {
        if (_tokenSets.TryGetValue(tokenSetName, out var value)) return value;
        throw new InvalidOperationException("Token set doesn't exist: " + tokenSetName);
    }

    public TokenSetComposed Compose(string tokenSetName)
    {
        var bottom = GetTokenSet(tokenSetName);
        if (bottom.Base == null)
        {
            return new TokenSetComposed
            {
                TokenSetName = bottom.TokenSetName,
                Tokens = bottom.Tokens.ToDictionary(b => b.Key, b => new TokenComposed
                {
                    Name = b.Key,
                    Token = b.Value.DeepClone(),
                    SourceTokenSet = bottom.TokenSetName,
                    TokenValueOrigin = TokenValueOrigin.Addition,
                    BaseToken = null
                }, StringComparer.OrdinalIgnoreCase)
            };
        }

        var bottomValues = new Dictionary<string, TokenComposed>();

        // initialize BOTTOM with all of the values from BASE.
        // All values are defaulted to INHERITED.
        var baseValues = Compose(bottom.Base).Tokens;
        foreach (var (k, v) in baseValues)
        {
            var copy = v.Clone();
            copy.TokenValueOrigin = TokenValueOrigin.Inherited;
            copy.BaseToken = v;
            bottomValues[k] = copy;
        }
        
        // now iterate all the tokens.
        // if a token already exists in the values, then replace it.
        // otherwise, add it.
        // things in the BASE but not CHILD will fall through.
        // things in the BASE and CHILD, CHILD wins.
        // things in CHILD only are added.
        foreach (var (key, value) in bottom.Tokens)
        {
            var newValue = new TokenComposed
            {
                Name = key,
                Token = value,
                SourceTokenSet = bottom.TokenSetName,
                TokenValueOrigin = TokenValueOrigin.Addition
            };

            // it was provided by the BASE, but child wins.
            if (bottomValues.ContainsKey(key))
            {
                // if it already exists, then it's in
                // the parent. so, this is an override.
                newValue.TokenValueOrigin = TokenValueOrigin.Override;
                newValue.BaseToken = bottomValues[key].BaseToken;
            }

            bottomValues[key] = newValue;
        }

        // remove doubles
        // each time a layer is resolved, it gets a new node
        // for every token.
        // GIVEN TS2 inherits from TS1 inherits from TS0
        // GIVEN TS0 defines a value A, with no overrides.
        //  TS0 resolves to     a=value, parent=null
        //  TS1 resolves to     a=value, parent=ts0   value is inherited
        //  TS2 resolves to     a=value, parent=ts1   value is inherited
        //  So, the value of ts2 links to ts1 which links to ts0
        //  This reduces it to: ts2 -> ts0
        foreach (var v in bottomValues.Values
                     .Where(v => v.BaseToken?.BaseToken != null))
        {
            if (string.Equals(v.BaseToken.SourceTokenSet, v.BaseToken.BaseToken.SourceTokenSet, StringComparison.OrdinalIgnoreCase))
            {
                v.BaseToken = v.BaseToken.BaseToken;
            }
        }
        
        return new TokenSetComposed
        {
            Base = bottom.Base,
            TokenSetName = bottom.TokenSetName,
            Tokens = bottomValues
        };
    }
}

public class BaseValue
{
    public string TokenSetName { get; set; }

    public JToken Value { get; set; }

    public BaseValue Parent { get; set; }
}

public class TokenSetComposed
{
    // TODO: add token sets
    
    
    public string? Base { get; set; }
    public string TokenSetName { get; set; }
    public Dictionary<string, TokenComposed> Tokens { get; set; }
}
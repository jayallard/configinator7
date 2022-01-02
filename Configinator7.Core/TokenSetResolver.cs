using Configinator7.Core.Model;
using Newtonsoft.Json.Linq;

namespace Configinator7.Core;

public class TokenSetResolver
{
    private readonly Dictionary<string, TokenSet> _tokenSets;

    public TokenSetResolver(IEnumerable<TokenSet> tokenSets)
    {
        _tokenSets = tokenSets.ToDictionary(t => t.TokenSetName, t => t, StringComparer.OrdinalIgnoreCase);
    }

    private TokenSet GetTokenSet(string tokenSetName)
    {
        if (_tokenSets.TryGetValue(tokenSetName, out var value)) return value;
        throw new InvalidOperationException("Token set doesn't exist: " + tokenSetName);
    }

    public TokenSetResolved Resolve(string tokenSetName)
    {
        var bottom = GetTokenSet(tokenSetName);
        if (bottom.Base == null)
        {
            return new TokenSetResolved
            {
                TokenSetName = bottom.TokenSetName,
                Tokens = bottom.Tokens.ToDictionary(b => b.Key, b => new TokenResolved
                {
                    Name = b.Key,
                    Value = b.Value.DeepClone(),
                    SourceTokenSet = bottom.TokenSetName,
                    Resolution = Resolution.Addition,
                    Parent = null
                }, StringComparer.OrdinalIgnoreCase)
            };
        }

        var bottomValues = new Dictionary<string, TokenResolved>();

        // initialize BOTTOM with all of the values from BASE.
        // All values are defaulted to INHERITED.
        var baseValues = Resolve(bottom.Base).Tokens;
        foreach (var (k, v) in baseValues)
        {
            var copy = v.Clone();
            copy.Resolution = Resolution.Inherited;
            copy.Parent = v;
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
            var newValue = new TokenResolved
            {
                Name = key,
                Value = value,
                SourceTokenSet = bottom.TokenSetName,
                Resolution = Resolution.Addition
            };

            // it was provided by the BASE, but child wins.
            if (bottomValues.ContainsKey(key))
            {
                // if it already exists, then it's in
                // the parent. so, this is an override.
                newValue.Resolution = Resolution.Override;
                newValue.Parent = bottomValues[key].Parent;
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
                     .Where(v => v.Parent?.Parent != null))
        {
            if (string.Equals(v.Parent.SourceTokenSet, v.Parent.Parent.SourceTokenSet, StringComparison.OrdinalIgnoreCase))
            {
                v.Parent = v.Parent.Parent;
            }
        }
        
        return new TokenSetResolved
        {
            Base = bottom.Base,
            TokenSetName = bottom.TokenSetName,
            Tokens = bottomValues
        };
    }
}

public enum Resolution
{
    Addition,
    Override,
    Inherited
}

public class TokenResolved
{
    public JToken Value { get; set; }

    public string Name { get; set; }

    public string SourceTokenSet { get; set; }

    public Resolution Resolution { get; set; }

    public TokenResolved? Parent { get; set; }

    public TokenResolved Clone() => new()
    {
        Value = Value.DeepClone(),
        Name = Name,
        SourceTokenSet = SourceTokenSet,
        Resolution = Resolution,
        Parent = Parent
    };
}

public class BaseValue
{
    public string TokenSetName { get; set; }

    public JToken Value { get; set; }

    public BaseValue Parent { get; set; }
}

public class TokenSetResolved
{
    public string Base { get; set; }
    public string TokenSetName { get; set; }
    public Dictionary<string, TokenResolved> Tokens { get; set; }
}
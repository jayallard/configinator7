using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class TokenSetComposed3
{
    private readonly Dictionary<string, JToken> _tokens;
    private readonly Dictionary<string, TokenComposed3> _resolved = new();
    private readonly List<TokenSetComposed3> _children = new();

    public Dictionary<string, JToken> Tokens =>
        _tokens.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.DeepClone(),
            StringComparer.OrdinalIgnoreCase);

    public ImmutableDictionary<string, TokenComposed3> TokensResolved
    {
        get
        {
            ResolveAll();
            return _resolved.ToImmutableDictionary(
                kv => kv.Key,
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase);
        }
    }

    private void ResolveAll()
    {
        foreach (var key in this.Keys) Resolve(key);
    }
    
    public TokenSetComposed3 Root => BaseTokenSet?.Root ?? this;

    public ISet<string> GetRelatedTokenSetNames()
    {
        var values = new HashSet<string>();

        // self and parents
        var current = this;
        while (current != null)
        {
            values.Add(current.TokenSetName);
            current = current.BaseTokenSet;
        }

        // descendants
        AddChildren(this);
        return values;

        void AddChildren(TokenSetComposed3 tokenSet)
        {
            foreach (var child in tokenSet.Children)
            {
                values.Add(child.TokenSetName);
                AddChildren(child);
            }
        }
    }

    internal void AddChild(TokenSetComposed3 child)
    {
        if (child.BaseTokenSet != null) throw new InvalidOperationException("bug");
        child.BaseTokenSet = this;
        _children.Add(child);
    }

    public IEnumerable<TokenSetComposed3> Children => _children.ToImmutableList();

    public TokenSetComposed3 GetChild(string tokenSetName) => _children.Single(c =>
        c.TokenSetName.Equals(tokenSetName, StringComparison.OrdinalIgnoreCase));

    public ISet<string> Keys =>
        BaseTokenSet == null
            ? _tokens.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : _tokens.Keys.Union(BaseTokenSet.Keys).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public string TokenSetName { get; }

    internal TokenComposed3? Resolve(string key)
    {
        // already resolved
        if (_resolved.ContainsKey(key)) return _resolved[key];
        var tokenComposed = new TokenComposed3(key)
        {
            TokenSet = this
        };
        var fromParent = BaseTokenSet?.Resolve(key);
        var existsInParent = fromParent != null;
        var existsHere = _tokens.ContainsKey(key);
        if (!existsHere && !existsInParent) return null;
        tokenComposed.Base = fromParent;

        // if it exists only here, then ti's ADDED (the default value)
        // if it exists here and in the parent, then it's an OVERRIDE
        // if it exists in the parent, but not here, then it's inherited
        if (existsInParent)
        {
            tokenComposed.Origin = existsHere
                ? TokenValueOrigin.Override
                : TokenValueOrigin.Inherited;
        }

        if (existsHere)
        {
            tokenComposed.Token = _tokens[key];
        }
        else if (existsInParent)
        {
            tokenComposed.Token = fromParent.Token;
        }

        _resolved[key] = tokenComposed;
        return tokenComposed;
    }

    public TokenComposed3 GetToken(string key)
    {
        var value = Resolve(key);
        if (value == null) throw new KeyNotFoundException(key);

        // todo: clone
        return value;
    }

    public TokenSetComposed3? BaseTokenSet { get; internal set; }

    public TokenSetComposed3(
        Dictionary<string, JToken> tokens,
        string tokenSetName)
    {
        TokenSetName = tokenSetName;
        _tokens = tokens.ToDictionary(
            kv => kv.Key,
            kv => kv.Value,
            StringComparer.OrdinalIgnoreCase);
    }
}
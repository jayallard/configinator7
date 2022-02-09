using System.Collections.ObjectModel;

namespace Allard.Json;

public class TokenSetComposed
{
    private readonly List<TokenSetComposed> _children = new();
    
    // TODO: add token sets
    public TokenSetComposed? Base { get; set; }

    public TokenSetComposed Root => Base == null ? this : Base.Root;

    public ReadOnlyCollection<TokenSetComposed> Children => _children.AsReadOnly();

    internal void AddChild(TokenSetComposed child) => _children.Add(child);
    
    public string TokenSetName { get; set; }
    public Dictionary<string, TokenComposed> Tokens { get; set; }

    // public TokenSetComposed Clone() => new()
    // {
    //     Base = Base,
    //     Tokens = Tokens.ToDictionary(kv => kv.Key, kv => kv.Value.Clone(), StringComparer.OrdinalIgnoreCase),
    //     TokenSetName = TokenSetName
    // };
}
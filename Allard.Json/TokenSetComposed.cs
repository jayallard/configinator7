namespace Allard.Json;

public class TokenSetComposed
{
    // TODO: add token sets
    public string? Base { get; set; }
    public string TokenSetName { get; set; }
    public Dictionary<string, TokenComposed> Tokens { get; set; }

    public TokenSetComposed Clone() => new()
    {
        Base = Base,
        Tokens = Tokens.ToDictionary(kv => kv.Key, kv => kv.Value.Clone(), StringComparer.OrdinalIgnoreCase),
        TokenSetName = TokenSetName
    };
}
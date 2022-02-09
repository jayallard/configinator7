using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class TokenSet
{
    public string? BaseTokenSetName { get; set; }
    public string TokenSetName { get; set; }

    public Dictionary<string, JToken> Tokens { get; set; } = new();

    public TokenSet Clone() => new()
    {
        BaseTokenSetName = BaseTokenSetName,
        TokenSetName = TokenSetName,
        Tokens = Tokens.ToDictionary(
            kv => kv.Key,
            kv => kv.Value,
            StringComparer.OrdinalIgnoreCase)
    };
}
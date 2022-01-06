using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class TokenSet
{
    public string? Base { get; set; }
    public string TokenSetName { get; set; }

    public Dictionary<string, JToken> Tokens { get; set; }
}
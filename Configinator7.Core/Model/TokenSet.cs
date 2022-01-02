using Newtonsoft.Json.Linq;

namespace Configinator7.Core.Model;

public class TokenSet
{
    public string? Base { get; set; }
    public string TokenSetName { get; set; }

    public Dictionary<string, JToken> Tokens { get; set; }
}
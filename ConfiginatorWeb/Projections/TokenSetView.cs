using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Projections;

public class TokenSetView
{
    public string Base { get; set; }
    public string TokenSetName { get; set; }
    public Dictionary<string, JToken> Tokens { get; set; }
}
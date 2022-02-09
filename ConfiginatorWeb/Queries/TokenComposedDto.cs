using Allard.Json;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Queries;

public class TokenComposedDto
{
    public JToken Token { get; set; }
    public string Name { get; set; }
    public string SourceTokenSet { get; set; }
    public TokenValueOrigin TokenValueOrigin { get; set; }
    public TokenComposedDto? BaseToken { get; set; }

    public static TokenComposedDto? FromTokenComposed(TokenComposed3? token)
    {
        if (token == null) return null;
        return new TokenComposedDto
        {
            BaseToken = FromTokenComposed(token.Base),
            Name = token.Name,
            SourceTokenSet ="TODO",
            Token = token.Token,
            TokenValueOrigin = token.Origin
        };
    }
}
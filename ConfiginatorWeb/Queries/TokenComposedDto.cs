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

    public static TokenComposedDto? FromTokenComposed(TokenComposed? token)
    {
        if (token == null) return null;
        return new TokenComposedDto
        {
            BaseToken = TokenComposedDto.FromTokenComposed(token.BaseToken),
            Name = token.Name,
            SourceTokenSet = token.SourceTokenSet,
            Token = token.Token.DeepClone(),
            TokenValueOrigin = token.TokenValueOrigin
        };
    }
}
using Allard.Json;

namespace ConfiginatorWeb.Queries;

public class TokenSetComposedDto
{
    public string? Base { get; set; }
    public string TokenSetName { get; set; }

    public long TokenSetId { get; set; }
    public Dictionary<string, TokenComposedDto> Tokens { get; set; }


    public static TokenSetComposedDto FromTokenSetComposed(TokenSetComposed3 tokenSet)
    {
        return new TokenSetComposedDto()
        {
            Base = tokenSet.BaseTokenSet?.TokenSetName,
            TokenSetName = tokenSet.TokenSetName,
            Tokens = tokenSet
                .TokensResolved
                .ToDictionary(
                    kv => kv.Key,
                    kv => ToDto(kv.Value),
                    StringComparer.OrdinalIgnoreCase)
        };

        TokenComposedDto ToDto(TokenComposed3 token)
        {
            return new TokenComposedDto
            {
                Name = token.Name,
                SourceTokenSetName = token.Root.TokenSet.TokenSetName,
                TokenSetName = token.TokenSet.TokenSetName,
                Token = token.Token,
                TokenValueOrigin = token.Origin,
                BaseToken = token.Base == null ? null : ToDto(token.Base)
            };
        }
    }
}
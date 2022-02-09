using Allard.Json;

namespace ConfiginatorWeb.Queries;

public class TokenSetComposedDto
{
    public string? Base { get; set; }
    public string TokenSetName { get; set; }
    
    public long TokenSetId { get; set; }
    public Dictionary<string, TokenComposedDto> Tokens { get; set; }


    public static TokenSetComposedDto FromTokenSetComposed(TokenSetComposed3 tokenSet) =>
        new()
        {
            Base = tokenSet.BaseTokenSet?.TokenSetName,
            TokenSetName = tokenSet.TokenSetName,
            Tokens = tokenSet
                .Tokens
                .ToDictionary(
                    kv => kv.Key, 
                    kv => new TokenComposedDto
                    {
                        
                    },
                    StringComparer.OrdinalIgnoreCase)
        };
}
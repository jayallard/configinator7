using Allard.Json;

namespace ConfiginatorWeb.Queries;

public class TokenSetComposedDto
{
    public string? Base { get; set; }
    public string TokenSetName { get; set; }
    
    public long TokenSetId { get; set; }
    public Dictionary<string, TokenComposedDto> Tokens { get; set; }


    public static TokenSetComposedDto FromTokenSetComposed(TokenSetComposed tokenSet) =>
        new()
        {
            Base = tokenSet.Base,
            TokenSetName = tokenSet.TokenSetName,
            Tokens = tokenSet
                .Tokens
                .ToDictionary(
                    kv => kv.Key, 
                    kv => TokenComposedDto.FromTokenComposed(kv.Value)!)
        };
}
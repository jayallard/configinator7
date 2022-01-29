namespace Allard.Json;

public class TokenSetComposed
{
    // TODO: add token sets
    public string? Base { get; set; }
    public string TokenSetName { get; set; }
    public Dictionary<string, TokenComposed> Tokens { get; set; }
}
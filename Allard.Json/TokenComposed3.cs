namespace Allard.Json;

public class TokenComposed3
{
    public string Name { get; }
    public TokenValueOrigin Origin { get; internal set; } = TokenValueOrigin.Defined;
    public TokenComposed3? Base { get; internal set; }
    public TokenComposed3(string name) => Name = name;
    
    public TokenSetComposed3 TokenSet { get; internal set; }
}
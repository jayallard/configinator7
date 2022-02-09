using Allard.Json;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Queries;

public class TokenComposedDto
{
    public JToken Token { get; set; }
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the token set in which the token
    /// is defined.
    /// IE: if the hierarchy is a->b->c->d.
    /// The token set was defined in C.
    /// The current token set is A.
    /// The value of this is C, because that's where the token was defined.
    /// </summary>
    public string SourceTokenSetName { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the token set that owns this token.
    /// IE: if the hierarchy is a->b->c->d.
    /// The token set was defined in C.
    /// The current token set is A.
    /// The value of this is A. The value of SourceTokenSetName is C.
    /// </summary>
    public string TokenSetName { get; set; }
    public TokenValueOrigin TokenValueOrigin { get; set; }
    public TokenComposedDto? BaseToken { get; set; }
}
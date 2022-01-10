using Newtonsoft.Json.Linq;

namespace Allard.Json;

/// <summary>
/// Token Sets have an inheritance hierarchy.
/// TokenComposed is the a token in the hierarchy, with a link
/// to it's base token.
/// For example: if TokenA is set to ABC, then is overridden to XYZ.
/// An instance of this exists for ABC and XYZ, and XYZ links to ABC.
/// </summary>
public class TokenComposed
{
    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    public JToken Token { get; set; }

    /// <summary>
    /// Gets or sets the name of the token.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the TokenSet that defined this token.
    /// </summary>
    public string SourceTokenSet { get; set; }

    /// <summary>
    /// Gets or sets the origin of the value.
    /// The value is either added, overridden or inherited.
    /// </summary>
    public TokenValueOrigin TokenValueOrigin { get; set; }

    /// <summary>
    /// Gets or sets the value of this token in it's base layer.
    /// </summary>
    public TokenComposed? BaseToken { get; set; }

    /// <summary>
    /// Creates a copy of the resolved token.
    /// </summary>
    /// <returns></returns>
    public TokenComposed Clone() => new()
    {
        Token = Token.DeepClone(),
        Name = Name,
        SourceTokenSet = SourceTokenSet,
        TokenValueOrigin = TokenValueOrigin,
        BaseToken = BaseToken
    };
}
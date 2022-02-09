using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class TokenComposed3
{
    public string Name { get; }
    public TokenValueOrigin Origin { get; internal set; } = TokenValueOrigin.Defined;
    
    /// <summary>
    /// Gets the tokens value in the base token set.
    /// IE: if the set hierarchy is   A -> B -> C -> D -> E
    /// The token is defined in D.
    /// A returns B.
    /// B returns C.
    /// C returns D.
    /// D returns null.
    /// </summary>
    public TokenComposed3? Base { get; internal set; }

    /// <summary>
    /// Gets the original token definition.
    /// IE: if the set hierarchy is   A -> B -> C -> D -> E
    /// The token is defined in D.
    /// Root is D.
    /// </summary>
    public TokenComposed3 Root => Base?.Root ?? this;
    public TokenComposed3(string name) => Name = name;
    public TokenSetComposed3 TokenSet { get; internal set; }
    public JToken Token { get; internal set; }
}
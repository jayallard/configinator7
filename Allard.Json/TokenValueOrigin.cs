namespace Allard.Json;

/// <summary>
/// TokenSets can inherit from another TokenSet.
/// </summary>
public enum TokenValueOrigin
{
    /// <summary>
    /// The value is added at this layer.
    /// </summary>
    Addition,
    
    /// <summary>
    /// This layer overrides a value of a higher layer.
    /// </summary>
    Override,
    
    /// <summary>
    /// The value is inherited from another layer.
    /// </summary>
    Inherited
}
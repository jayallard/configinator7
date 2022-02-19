namespace Allard.Json;

/// <summary>
///     A VariableSet can inherit from another VariableSet.
/// </summary>
public enum VariableOrigin
{
    /// <summary>
    ///     The value is added at this layer.
    /// </summary>
    Defined,

    /// <summary>
    ///     This layer overrides a value of a higher layer.
    /// </summary>
    Override,

    /// <summary>
    ///     The value is inherited from another layer.
    /// </summary>
    Inherited
}
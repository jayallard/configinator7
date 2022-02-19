using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class VariableComposed
{
    public VariableComposed(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public VariableOrigin Origin { get; internal set; } = VariableOrigin.Defined;

    /// <summary>
    ///     Gets the variable's value in the base variable set.
    ///     IE: if the set hierarchy is   A -> B -> C -> D -> E
    ///     The variable is defined in D.
    ///     A returns B.
    ///     B returns C.
    ///     C returns D.
    ///     D returns null.
    /// </summary>
    public VariableComposed? Base { get; internal set; }

    /// <summary>
    ///     Gets the original variable definition.
    ///     IE: if the set hierarchy is   A -> B -> C -> D -> E
    ///     The variable is defined in D.
    ///     Root is D.
    /// </summary>
    public VariableComposed Root => Base?.Root ?? this;

    public VariableSetComposed VariableSet { get; internal set; }
    public JToken Value { get; internal set; }
}
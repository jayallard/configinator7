using Allard.Json;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Queries;

public class VariableComposedDto
{
    public JToken Value { get; set; }
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the variable set in which the variable
    /// is defined.
    /// IE: if the hierarchy is a->b->c->d.
    /// The variable set was defined in C.
    /// The current variable set is A.
    /// The value of this is C, because that's where the variable was defined.
    /// </summary>
    public string SourceVariableSetName { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the variable set that owns this variable.
    /// IE: if the hierarchy is a->b->c->d.
    /// The variable set was defined in C.
    /// The current variable set is A.
    /// The value of this is A. The value of SourceVariableSetName is C.
    /// </summary>
    public string VariableSetName { get; set; }
    public VariableOrigin VariableOrigin { get; set; }
    public VariableComposedDto? BaseToken { get; set; }
}
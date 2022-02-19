using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class VariableSet
{
    public string? BaseVariableSetName { get; set; }
    public string VariableSetName { get; set; }

    public Dictionary<string, JToken> Variables { get; set; } = new();

    public VariableSet Clone()
    {
        return new()
        {
            BaseVariableSetName = BaseVariableSetName,
            VariableSetName = VariableSetName,
            Variables = Variables.ToDictionary(
                kv => kv.Key,
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase)
        };
    }
}
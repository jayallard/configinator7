using Allard.Json;

namespace ConfiginatorWeb.Queries;

public class VariableSetComposedDto
{
    public string? Base { get; set; }
    public string VariableSetName { get; set; }
    public long VariableSetId { get; set; }
    public string Namespace { get; set; }
    public Dictionary<string, VariableComposedDto> Variables { get; set; }

    public static VariableSetComposedDto FromVariableSetComposed(VariableSetComposed variableSet)
    {
        return new VariableSetComposedDto
        {
            Base = variableSet.BaseVariableSet?.VariableSetName,
            VariableSetName = variableSet.VariableSetName,
            Variables = variableSet
                .VariablesResolved
                .ToDictionary(
                    kv => kv.Key,
                    kv => ToDto(kv.Value),
                    StringComparer.OrdinalIgnoreCase)
        };

        VariableComposedDto ToDto(VariableComposed variable)
        {
            return new VariableComposedDto
            {
                Name = variable.Name,
                SourceVariableSetName = variable.Root.VariableSet.VariableSetName,
                VariableSetName = variable.VariableSet.VariableSetName,
                Value = variable.Value,
                VariableOrigin = variable.Origin,
                BaseToken = variable.Base == null ? null : ToDto(variable.Base)
            };
        }
    }
}
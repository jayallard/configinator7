using ConfiginatorWeb.Queries;

namespace ConfiginatorWeb.Models.Release;

public class EditReleaseView
{
    public long SectionId { get; set; }
    public string SectionName { get; set; }

    public string Namespace { get; set; }
    public string EnvironmentName { get; set; }
    
    public long EnvironmentId { get; set; }
    public List<EditSchemaView> Schemas { get; set; }
    public string? DefaultValue { get; set; }
    public List<EditSchemaVariableView> VariableSet { get; set; }
    public string? DefaultVariableSetName { get; set; }
    public string? DefaultSchemaName { get; set; }
}

public record EditSchemaView(SchemaNameDto SchemaName, long SchemaId, string Schema);

public record EditSchemaVariableView(string VariableSetName, long VariableSetId);


using Allard.Configinator.Core.Model;
using ConfiginatorWeb.Queries;

namespace ConfiginatorWeb.Models.Release;

public class EditReleaseView
{
    public string SectionName { get; set; }
    public string EnvironmentName { get; set; }
    public List<EditSchemaView> Schemas { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> VariableSetNames { get; set; }
    public string? DefaultVariableSetName { get; set; }
    public string? DefaultSchemaName { get; set; }
}
public record EditSchemaView(SchemaNameDto SchemaName, string Schema);

public class DisplayView
{
    public SchemaNameDto SectionName { get; set; }
    public string EnvironmentName { get; set; }
    public long ReleaseId { get; set; }
    public ReleaseEntity Release { get; set; }
}


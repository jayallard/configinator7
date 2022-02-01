using Allard.Configinator.Core.Model;

namespace ConfiginatorWeb.Models.Release;

public class EditReleaseView
{
    public string SectionName { get; set; }
    public string EnvironmentName { get; set; }
    public List<EditSchemaView> Schemas { get; set; }
    
    public string? DefaultValue { get; set; }
    public List<string> TokenSetNames { get; set; }
    
    public string? DefaultTokenSetName { get; set; }
}
public record EditSchemaView(string Id, string Version, string Schema);

public class DisplayView
{
    public string SectionName { get; set; }
    public string EnvironmentName { get; set; }
    public long ReleaseId { get; set; }
    public ReleaseEntity Release { get; set; }
}


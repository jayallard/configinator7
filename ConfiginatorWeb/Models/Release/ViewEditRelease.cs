using Configinator7.Core.Model;

namespace ConfiginatorWeb.Models.Release;

public class ViewEditRelease
{
    public string SectionName { get; set; }
    public string EnvironmentName { get; set; }
    public List<ViewSchema> Schemas { get; set; }
}
public record ViewSchema(string Id, string Version, string Schema);
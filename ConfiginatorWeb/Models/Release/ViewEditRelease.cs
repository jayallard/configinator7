using Configinator7.Core.Model;

namespace ConfiginatorWeb.Models.Release;

public class ViewEditRelease
{
    public string ConfigurationSectionName { get; set; }
    public string HabitatName { get; set; }
    public List<ViewSchema> Schemas { get; set; }
}
public record ViewSchema(string Id, string Version, string Schema);
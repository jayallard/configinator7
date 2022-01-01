using Configinator7.Core.Model;

namespace ConfiginatorWeb.Projections;

public class ConfigurationItemView
{
    public SectionId SectionId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
}
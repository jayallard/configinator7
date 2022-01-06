using Allard.Configinator.Core.Model;

namespace ConfiginatorWeb.Projections;

public class ConfigurationSectionView
{
    public SectionId SectionId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    
    public bool DeployedIsOutOfDate { get; set; }
}
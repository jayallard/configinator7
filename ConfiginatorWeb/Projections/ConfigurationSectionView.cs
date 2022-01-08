using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;

namespace ConfiginatorWeb.Projections;

public class ConfigurationSectionView
{
    public long SectionId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    
    public bool DeployedIsOutOfDate { get; set; }
}
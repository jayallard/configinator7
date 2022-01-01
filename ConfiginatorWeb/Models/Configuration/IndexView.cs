using ConfiginatorWeb.Projections;

namespace ConfiginatorWeb.Models.Configuration;

public class IndexView
{
    public IEnumerable<ConfigurationSectionView> ConfigurationSections { get; set; }
    public IEnumerable<TokenSetView> TokenSets { get; set; }
}
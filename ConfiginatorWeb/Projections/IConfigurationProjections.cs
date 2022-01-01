namespace ConfiginatorWeb.Projections;

public interface IConfigurationProjections
{
    public IEnumerable<ConfigurationSectionView> GetSections();
    public IEnumerable<TokenSetView> GetTokenSets();
}
namespace ConfiginatorWeb.Projections;

public interface IConfigurationProjections
{
    public IEnumerable<ConfigurationItemView> GetConfigurationSections();
}
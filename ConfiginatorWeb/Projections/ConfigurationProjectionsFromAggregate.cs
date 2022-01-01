using Configinator7.Core.Model;

namespace ConfiginatorWeb.Projections;

public class ConfigurationProjectionsFromAggregate : IConfigurationProjections
{
    private readonly SuperAggregate _aggregate;

    public ConfigurationProjectionsFromAggregate(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    public IEnumerable<ConfigurationItemView> GetConfigurationSections() =>
        _aggregate.TemporaryExposure.Values.Select(v => new ConfigurationItemView
        {
            ConfigurationSectionId = v.Id,
            Name = v.Id.Name,
            Path = v.Path
        });
}
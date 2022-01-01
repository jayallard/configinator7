using Configinator7.Core.Model;

namespace ConfiginatorWeb.Projections;

public class ConfigurationProjectionsFromAggregate : IConfigurationProjections
{
    private readonly SuperAggregate _aggregate;

    public ConfigurationProjectionsFromAggregate(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    public IEnumerable<ConfigurationItemView> GetSections() =>
        _aggregate.TemporaryExposure.Values.Select(v => new ConfigurationItemView
        {
            SectionId = v.Id,
            Name = v.Id.Name,
            Path = v.Path
        });
}
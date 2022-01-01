using Configinator7.Core.Model;

namespace ConfiginatorWeb.Projections;

public class ConfigurationProjectionsFromAggregate : IConfigurationProjections
{
    private readonly SuperAggregate _aggregate;

    public ConfigurationProjectionsFromAggregate(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    public IEnumerable<ConfigurationSectionView> GetSections() =>
        _aggregate.TemporaryExposureSections.Values.Select(v => new ConfigurationSectionView
        {
            SectionId = v.Id,
            Name = v.Id.Name,
            Path = v.Path
        });

    public IEnumerable<TokenSetView> GetTokenSets() =>
        _aggregate.TemporaryExposureTokenSets.Select(s => new TokenSetView
        {
            Tokens = s.Value.Tokens.ToDictionary(t => t.Key, t => t.Value.DeepClone()),
            TokenSetName = s.Value.Name,
            Base = s.Value.Base
        });
}
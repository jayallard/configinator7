using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class SectionQueriesCoreRepository : ISectionQueries
{
    private readonly ISectionRepository _sectionRepository;
    private readonly ITokenSetRepository _tokenSetRepository;

    public SectionQueriesCoreRepository(ISectionRepository sectionRepository, ITokenSetRepository tokenSetRepository)
    {
        _sectionRepository = Guards.NotDefault(sectionRepository, nameof(sectionRepository));
        _tokenSetRepository = Guards.NotDefault(tokenSetRepository, nameof(tokenSetRepository));
    }

    public async Task<List<SectionListItemDto>> GetSectionsListAsync(CancellationToken cancellationToken = default)
        => (await _sectionRepository.FindAsync(new AllSections(), cancellationToken))
            .Select(s => new SectionListItemDto(s.Id.Id, s.SectionName, s.Path))
            .ToList();

    public async Task<SectionDto?> GetSectionAsync(long id, CancellationToken cancellationToken = default)
    {
        var section = await _sectionRepository.GetAsync(new SectionId(id), cancellationToken);
        return await CreateSectionDto(section, cancellationToken);
    }

    public async Task<SectionDto> GetSectionAsync(string name, CancellationToken cancellationToken = default)
    {
        // get the section
        var section = (await _sectionRepository.FindAsync(new SectionNameIs(name), cancellationToken))
            .Single();
        return await CreateSectionDto(section, cancellationToken);
    }

    private async Task<SectionDto> CreateSectionDto(SectionAggregate section, CancellationToken cancellationToken)
    {
        var tokenSets = (await _tokenSetRepository.FindAsync(new All(), cancellationToken))
            .ToDictionary(ts => ts.Id, ts => ts);
        var composer = new TokenSetComposer(tokenSets.Values.Select(ts => ts.ToTokenSet()));
        
        var sectionDto = section.ToOutputDto();
        
        // iterate all releases and assign the tokens
        foreach (var env in section.Environments)
        {
            var envDto = sectionDto.GetEnvironment(env.EnvironmentName);
            foreach (var release in env.Releases.Where(r => r.TokenSetId != null))
            {
                var composed = composer.Compose(tokenSets[release.TokenSetId!].TokenSetName);
                envDto.GetRelease(release.Id.Id).TokenSet = TokenSetComposedDto.FromTokenSetComposed(composed);
            }
        }

        return sectionDto;
    }
}
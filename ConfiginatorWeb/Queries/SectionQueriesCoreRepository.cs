using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class SectionQueriesCoreRepository : ISectionQueries
{
    private readonly ISectionRepository _repository;

    public SectionQueriesCoreRepository(ISectionRepository repository) =>
        _repository = Guards.NotDefault(repository, nameof(repository));

    public async Task<List<SectionListItemDto>> GetSectionsListAsync(CancellationToken cancellationToken = default)
        => (await _repository.FindAsync(new AllSections(), cancellationToken))
            .Select(s => new SectionListItemDto(s.Id.Id, s.SectionName, s.Path, s.TokenSetName))
            .ToList();

    public async Task<SectionDto?> GetSectionAsync(long id, CancellationToken cancellationToken = default) =>
        (await _repository.GetAsync(new SectionId(id), cancellationToken))?.ToOutputDto();

    public async Task<SectionDto?> GetSectionAsync(string name, CancellationToken cancellationToken = default) =>
        (await _repository.FindAsync(new SectionNameIs(name), cancellationToken)).SingleOrDefault()?.ToOutputDto();
}
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class SectionQueriesDatabase : ISectionQueries
{
    private readonly ISectionRepository _repository;

    public SectionQueriesDatabase(ISectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SectionListItemView>> GetSectionsListAsync(CancellationToken cancellationToken = default)
        => (await _repository
                .FindAsync(new AllSections()))
            .Select(s => new SectionListItemView(s.Id.Id, s.SectionName, s.Path, s.TokenSetName))
            .ToList();

    public async Task<SectionView?> GetSectionAsync(long id, CancellationToken cancellationToken = default) =>
        (await _repository.GetAsync(new SectionId(id), cancellationToken))?.ToOutputDto();

    public async Task<SectionView?> GetSectionAsync(string name, CancellationToken cancellationToken = default) =>
        (await _repository.FindAsync(new SectionNameIs(name), cancellationToken)).SingleOrDefault()?.ToOutputDto();
}
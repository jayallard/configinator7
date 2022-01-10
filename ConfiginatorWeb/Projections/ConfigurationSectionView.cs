using Allard.Configinator.Core.Repositories;

namespace ConfiginatorWeb.Projections;

public interface ISectionsProjections
{
    Task<IEnumerable<SectionView>> GetSectionsListAsync(CancellationToken cancellationToken = default);
}

public record SectionView(long SectionId, string Name, string Path, string TokenSetName);

public class SectionsProjectionsRepository : ISectionsProjections
{
    private readonly ISectionRepository _repository;

    public SectionsProjectionsRepository(ISectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SectionView>> GetSectionsListAsync(CancellationToken cancellationToken = default)
    {
        return (await _repository.GetSectionsAsync(cancellationToken))
            .Select(s => new SectionView(s.Id.Id, s.SectionName, s.Path, s.TokenSetName))
            .ToList();
    }
}
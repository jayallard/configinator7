namespace ConfiginatorWeb.Queries;

public interface ISectionQueries
{
    Task<List<SectionListItemView>> GetSectionsListAsync(CancellationToken cancellationToken = default);

    Task<SectionView> GetSectionAsync(long id, CancellationToken cancellationToken = default);
}
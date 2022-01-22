namespace ConfiginatorWeb.Queries;

public interface ISectionQueries
{
    Task<List<SectionView>> GetSectionsListAsync(CancellationToken cancellationToken = default);
}
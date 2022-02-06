namespace ConfiginatorWeb.Queries;

public interface ISectionQueries
{
    Task<List<SectionListItemDto>> GetSectionsListAsync(CancellationToken cancellationToken = default);
    Task<SectionDto> GetSectionAsync(long id, CancellationToken cancellationToken = default);
    Task<SectionDto> GetSectionAsync(string name, CancellationToken cancellationToken = default);
}
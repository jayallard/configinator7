namespace ConfiginatorWeb.Queries;

public interface ITokenSetQueries
{
    Task<List<TokenSetListItemDto>> GetTokenSetListAsync(CancellationToken cancellationToken = default);
    //Task<TokenSetComposedDto?> GetTokenSetAsync(string tokenSetName, CancellationToken cancellationToken = default);
}
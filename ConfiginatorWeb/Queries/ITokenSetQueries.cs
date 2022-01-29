namespace ConfiginatorWeb.Queries;

public interface ITokenSetQueries
{
    Task<List<TokenSetListItemDto>> GetTokenSetListAsync(CancellationToken cancellationToken = default);
}
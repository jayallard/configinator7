namespace ConfiginatorWeb.Queries;

public interface IVariableSetQueries
{
    Task<List<VariableSetListItemDto>> GetVariableSetListAsync(CancellationToken cancellationToken = default);
}
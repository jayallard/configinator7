using ConfiginatorWeb.Controllers;

namespace ConfiginatorWeb.Queries;

public interface IGlobalSchemaQueries
{
    Task<List<GlobalSchemaListItemDto>> GetGlobalSchemasListAsync(CancellationToken cancellationToken = default);
}
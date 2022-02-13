using ConfiginatorWeb.Interactors;
using ConfiginatorWeb.Interactors.Configuration;

namespace ConfiginatorWeb.Queries;

public interface IGlobalSchemaQueries
{
    Task<List<GlobalSchemaListItemDto>> GetGlobalSchemasListAsync(CancellationToken cancellationToken = default);
}
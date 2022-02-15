using ConfiginatorWeb.Interactors;
using ConfiginatorWeb.Interactors.Section;

namespace ConfiginatorWeb.Queries;

public interface IGlobalSchemaQueries
{
    Task<List<GlobalSchemaListItemDto>> GetGlobalSchemasListAsync(CancellationToken cancellationToken = default);
}
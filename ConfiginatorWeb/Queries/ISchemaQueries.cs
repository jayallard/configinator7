using ConfiginatorWeb.Interactors.Queries.Section;

namespace ConfiginatorWeb.Queries;

public interface ISchemaQueries
{
    Task<List<SchemaListItemDto>> GetSchemasListAsync(CancellationToken cancellationToken = default);
}
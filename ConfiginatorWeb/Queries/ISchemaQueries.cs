using ConfiginatorWeb.Interactors.Section;

namespace ConfiginatorWeb.Queries;

public interface ISchemaQueries
{
    Task<List<SchemaListItemDto>> GetSchemasListAsync(CancellationToken cancellationToken = default);
}
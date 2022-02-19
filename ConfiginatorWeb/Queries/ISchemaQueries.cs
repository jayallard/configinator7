using ConfiginatorWeb.Interactors.Section;

namespace ConfiginatorWeb.Queries;

public interface ISchemaQueries
{
    Task<List<SchemaListItemDto>> GetGlobalSchemasListAsync(CancellationToken cancellationToken = default);

    Task<List<SchemaListItemDto>> GetSectionSchemasListAsync(long sectionId,
        CancellationToken cancellationToken = default);
}
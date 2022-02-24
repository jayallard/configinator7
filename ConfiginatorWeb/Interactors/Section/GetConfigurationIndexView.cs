using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors.Section;

public class ConfigurationIndexRequestHandler : IRequestHandler<ConfigurationIndexRequest, ConfigurationIndexResponse>
{
    private readonly ISchemaQueries _schemaQueries;
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;

    public ConfigurationIndexRequestHandler(ISectionQueries sectionQueries, IVariableSetQueries variableSetQueries,
        ISchemaQueries schemaQueries)
    {
        _sectionQueries = sectionQueries;
        _variableSetQueries = variableSetQueries;
        _schemaQueries = schemaQueries;
    }

    public async Task<ConfigurationIndexResponse> Handle(ConfigurationIndexRequest request,
        CancellationToken cancellationToken)
    {
        var sections = _sectionQueries.GetSectionsListAsync(cancellationToken);
        var variableSets = _variableSetQueries.GetVariableSetListAsync(cancellationToken);
        var schemas = _schemaQueries.GetSchemasListAsync(cancellationToken);
        return new ConfigurationIndexResponse(await sections, await variableSets, (await schemas).ToList());
    }
}

public record ConfigurationIndexRequest : IRequest<ConfigurationIndexResponse>;

public record ConfigurationIndexResponse(
    List<SectionListItemDto> Sections,
    List<VariableSetListItemDto> VariableSets,
    List<SchemaListItemDto> Schemas) : IRequest<CreateSectionAppResponse>;

public record SchemaListItemDto(long SchemaId, long? SectionId, string Namespace, SchemaNameDto SchemaName, ISet<string> EnvironmentTypes,
    string? Description);
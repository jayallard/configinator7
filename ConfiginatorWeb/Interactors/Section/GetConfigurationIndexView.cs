using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors.Section;

public class ConfigurationIndexRequestHandler : IRequestHandler<ConfigurationIndexRequest, ConfigurationIndexResponse>
{
    private readonly ISchemaQueries _schemaQueries;
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;
    private readonly INamespaceQueries _namespaceQueries;

    public ConfigurationIndexRequestHandler(ISectionQueries sectionQueries, IVariableSetQueries variableSetQueries,
        ISchemaQueries schemaQueries, INamespaceQueries namespaceQueries)
    {
        _sectionQueries = sectionQueries;
        _variableSetQueries = variableSetQueries;
        _schemaQueries = schemaQueries;
        _namespaceQueries = namespaceQueries;
    }

    public async Task<ConfigurationIndexResponse> Handle(ConfigurationIndexRequest request,
        CancellationToken cancellationToken)
    {
        var sections = _sectionQueries.GetSectionsListAsync(cancellationToken);
        var variableSets = _variableSetQueries.GetVariableSetListAsync(cancellationToken);
        var schemas = _schemaQueries.GetSchemasListAsync(cancellationToken);
        var ns = _namespaceQueries.GetNamespaces();
        
        return new ConfigurationIndexResponse(await ns, await sections, await variableSets, (await schemas).ToList());
    }
}

public record ConfigurationIndexRequest : IRequest<ConfigurationIndexResponse>;

public record ConfigurationIndexResponse(
    List<NamespaceDto> Namespaces,
    List<SectionListItemDto> Sections,
    List<VariableSetListItemDto> VariableSets,
    List<SchemaListItemDto> Schemas) : IRequest<CreateSectionAppResponse>;

public record SchemaListItemDto(long SchemaId, long? SectionId, string Namespace, SchemaNameDto SchemaName, ISet<string> EnvironmentTypes,
    string? Description);
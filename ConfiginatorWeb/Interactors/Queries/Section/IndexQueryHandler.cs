using ConfiginatorWeb.Interactors.Commands.Section;
using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors.Queries.Section;

public class IndexQueryHandler : IRequestHandler<IndexRequest, IndexResponse>
{
    private readonly INamespaceQueries _namespaceQueries;
    private readonly ISchemaQueries _schemaQueries;
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;

    public IndexQueryHandler(ISectionQueries sectionQueries, IVariableSetQueries variableSetQueries,
        ISchemaQueries schemaQueries, INamespaceQueries namespaceQueries)
    {
        _sectionQueries = sectionQueries;
        _variableSetQueries = variableSetQueries;
        _schemaQueries = schemaQueries;
        _namespaceQueries = namespaceQueries;
    }

    public async Task<IndexResponse> Handle(IndexRequest request,
        CancellationToken cancellationToken)
    {
        var sections = _sectionQueries.GetSectionsListAsync(cancellationToken);
        var variableSets = _variableSetQueries.GetVariableSetListAsync(cancellationToken);
        var schemas = _schemaQueries.GetSchemasListAsync(cancellationToken);
        var ns = _namespaceQueries.GetNamespaces();

        return new IndexResponse(await ns, await sections, await variableSets, (await schemas).ToList());
    }
}

public record IndexRequest : IRequest<IndexResponse>;

public record IndexResponse(
    List<NamespaceDto> Namespaces,
    List<SectionListItemDto> Sections,
    List<VariableSetListItemDto> VariableSets,
    List<SchemaListItemDto> Schemas) : IRequest<CreateSectionAppResponse>;

public record SchemaListItemDto(long SchemaId, long? SectionId, string Namespace, SchemaNameDto SchemaName,
    ISet<string> EnvironmentTypes,
    string? Description);
using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors.Section;

public class ConfigurationIndexRequestHandler : IRequestHandler<ConfigurationIndexRequest, ConfigurationIndexResponse>
{
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;
    private readonly IGlobalSchemaQueries _globalSchemaQueries;

    public ConfigurationIndexRequestHandler(ISectionQueries sectionQueries, IVariableSetQueries variableSetQueries,
        IGlobalSchemaQueries globalSchemaQueries)
    {
        _sectionQueries = sectionQueries;
        _variableSetQueries = variableSetQueries;
        _globalSchemaQueries = globalSchemaQueries;
    }

    public async Task<ConfigurationIndexResponse> Handle(ConfigurationIndexRequest request, CancellationToken cancellationToken)
    {
        var sections = _sectionQueries.GetSectionsListAsync(cancellationToken);
        var variableSets = _variableSetQueries.GetVariableSetListAsync(cancellationToken);
        var schemas = _globalSchemaQueries.GetGlobalSchemasListAsync(cancellationToken);
        return new ConfigurationIndexResponse(await sections, await variableSets, (await schemas).ToList());
    }
}

public record ConfigurationIndexRequest : IRequest<ConfigurationIndexResponse>;
public record ConfigurationIndexResponse(
    List<SectionListItemDto> Sections,
    List<VariableSetListItemDto> VariableSets,
    List<GlobalSchemaListItemDto> GlobalSchemas) : IRequest<CreateSectionAppResponse>;


public record GlobalSchemaListItemDto(long GlobalSchemaId, string Name, ISet<string> EnvironmentTypes, string? Description);
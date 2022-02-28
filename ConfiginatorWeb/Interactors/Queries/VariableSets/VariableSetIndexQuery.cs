using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors.Queries.VariableSets;

public class VariableSetIndexQuery : IRequestHandler<VariableSetIndexQueryRequest, VariableSetIndexQueryResponse>
{
    private readonly VariableSetDomainService _variableSetDomainService;

    public VariableSetIndexQuery(VariableSetDomainService variableSetDomainService)
    {
        _variableSetDomainService = Guards.HasValue(variableSetDomainService, nameof(variableSetDomainService));
    }

    public async Task<VariableSetIndexQueryResponse> Handle(VariableSetIndexQueryRequest request,
        CancellationToken cancellationToken)
    {
        var variableSetComposed =
            await _variableSetDomainService.GetVariableSetComposedAsync(request.VariableSetName, cancellationToken);
        var mermaid = MermaidUtility.FlowChartForVariableSet(variableSetComposed, request.VariableSetName);
        var dto = VariableSetComposedDto.FromVariableSetComposed(variableSetComposed);
        return new VariableSetIndexQueryResponse(dto, mermaid);
    }
}

public record VariableSetIndexQueryRequest(string VariableSetName) : IRequest<VariableSetIndexQueryResponse>;

public record VariableSetIndexQueryResponse(VariableSetComposedDto VariableSet, string MermaidMarkup);
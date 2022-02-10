using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ConfiginatorWeb.Interactors;

public class GetVariableSetComposedQuery : IRequestHandler<VariableSetComposedQuery, VariableSetComposedQueryResult>
{
    private readonly VariableSetDomainService _variableSetDomainService;

    public GetVariableSetComposedQuery(VariableSetDomainService variableSetDomainService)
    {
        _variableSetDomainService = Guards.NotDefault(variableSetDomainService, nameof(variableSetDomainService));
    }

    public async Task<VariableSetComposedQueryResult> Handle(VariableSetComposedQuery request,
        CancellationToken cancellationToken)
    {
        var variableSetComposed = await _variableSetDomainService.GetVariableSetComposedAsync(request.VariableSetName, cancellationToken);
        var mermaid = MermaidUtility.FlowChartForVariableSet(variableSetComposed, request.VariableSetName);
        var dto = VariableSetComposedDto.FromVariableSetComposed(variableSetComposed);
        return new VariableSetComposedQueryResult(dto, mermaid);
    }
}


public record VariableSetComposedQuery(string VariableSetName) : IRequest<VariableSetComposedQueryResult>;

public record VariableSetComposedQueryResult(VariableSetComposedDto VariableSet, string MermaidMarkup);
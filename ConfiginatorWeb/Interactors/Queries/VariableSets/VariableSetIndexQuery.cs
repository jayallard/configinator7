using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors.Queries.VariableSets;

public class VariableSetIndexQuery : IRequestHandler<VariableSetIndexQueryRequest, VariableSetIndexQueryResponse>
{
    private readonly VariableSetDomainService _variableSetDomainService;
    private readonly IUnitOfWork _uow;

    public VariableSetIndexQuery(VariableSetDomainService variableSetDomainService, IUnitOfWork uow)
    {
        _uow = uow;
        _variableSetDomainService = Guards.HasValue(variableSetDomainService, nameof(variableSetDomainService));
    }

    public async Task<VariableSetIndexQueryResponse> Handle(VariableSetIndexQueryRequest request,
        CancellationToken cancellationToken)
    {
        var vs = await _uow.VariableSets.FindOneAsync(new VariableSetNameIs(request.VariableSetName),
            cancellationToken);
        var variableSetComposed =
            await _variableSetDomainService.GetVariableSetComposedAsync(request.VariableSetName, cancellationToken);
        var mermaid = MermaidUtility.FlowChartForVariableSet(variableSetComposed, request.VariableSetName);
        var dto = VariableSetComposedDto.FromVariableSetComposed(variableSetComposed);
        dto.Namespace = vs.Namespace;
        return new VariableSetIndexQueryResponse(dto, mermaid);
    }
}

public record VariableSetIndexQueryRequest(string VariableSetName) : IRequest<VariableSetIndexQueryResponse>;

public record VariableSetIndexQueryResponse(VariableSetComposedDto VariableSet, string MermaidMarkup);
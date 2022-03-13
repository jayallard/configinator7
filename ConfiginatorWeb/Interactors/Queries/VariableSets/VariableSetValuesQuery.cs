using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using MediatR;

namespace ConfiginatorWeb.Interactors.Queries.VariableSets;

public class VariableSetValuesQuery : IRequestHandler<VariableSetValuesQueryRequest, VariableSetValuesQueryResponse>
{
    private readonly VariableSetDomainService _service;

    public VariableSetValuesQuery(VariableSetDomainService service)
    {
        _service = service;
    }

    public async Task<VariableSetValuesQueryResponse> Handle(VariableSetValuesQueryRequest request,
        CancellationToken cancellationToken)
    {
        var variables = await _service.GetVariableSetComposedAsync(request.VariableSetName, cancellationToken);
        var values = variables.ToValueDictionary()
            .Select(kv => new VariableSetValuesQueryResponse.VariableValue(kv.Key, kv.Value.ToString()))
            .ToArray();
        return new VariableSetValuesQueryResponse(request.VariableSetName, values);
    }
}

public record VariableSetValuesQueryRequest(string VariableSetName) : IRequest<VariableSetValuesQueryResponse>;

public record VariableSetValuesQueryResponse(string VariableSetName,
    VariableSetValuesQueryResponse.VariableValue[] Values)
{
    public record VariableValue(string Name, string Value);
}
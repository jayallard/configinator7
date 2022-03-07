using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;

namespace ConfiginatorWeb.Queries;

public class VariableSetQueriesCoreRepository : IVariableSetQueries
{
    private readonly IVariableSetRepository _repository;
    private readonly VariableSetDomainService _variableSetDomainService;

    public VariableSetQueriesCoreRepository(IVariableSetRepository repository,
        VariableSetDomainService variableSetDomainService)
    {
        _variableSetDomainService = Guards.HasValue(variableSetDomainService, nameof(variableSetDomainService));
        _repository = Guards.HasValue(repository, nameof(repository));
    }

    public async Task<List<VariableSetListItemDto>> GetVariableSetListAsync(
        CancellationToken cancellationToken = default)
    {
        var list = (await _repository.FindAsync(new All(), cancellationToken))
            .Select(async t =>
            {
                // create mermaid diagram for each root variable set
                // it's a root if it doesn't have a base
                if (t.BaseVariableSetName != null)
                    return new VariableSetListItemDto(
                        t.EntityId,
                        t.Namespace, 
                        t.VariableSetName, 
                        t.EnvironmentType,
                        t.BaseVariableSetName,
                        null);
                var composed =
                    await _variableSetDomainService.GetVariableSetComposedAsync(t.VariableSetName, cancellationToken);
                var mermaid = MermaidUtility.FlowChartForVariableSet(composed, t.VariableSetName);
                return new VariableSetListItemDto(
                    t.EntityId,
                    t.Namespace, 
                    t.VariableSetName, 
                    t.EnvironmentType,
                    t.BaseVariableSetName, 
                    mermaid);
            }).ToList();
        await Task.WhenAll(list);
        return list.Select(l => l.Result).ToList();
    }
}
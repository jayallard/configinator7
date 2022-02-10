using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;

namespace Allard.Configinator.Core.DomainServices;

public class VariableSetDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public VariableSetDomainService(IUnitOfWork unitOfWork, IIdentityService identityService)
    {
        _unitOfWork = Guards.NotDefault(unitOfWork, nameof(unitOfWork));
        _identityService = Guards.NotDefault(identityService, nameof(identityService));
    }

    public async Task<VariableSetAggregate> CreateVariableSetAsync(string variableSetName, string? baseVariableSetName = default, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.VariableSets.Exists(new VariableSetNameIs(variableSetName)))
        {
            throw new InvalidOperationException("VariableSet already exists: " + variableSetName);
        }

        if (baseVariableSetName is not null && !(await _unitOfWork.VariableSets.Exists(new VariableSetNameIs(baseVariableSetName))))
        {
            throw new InvalidOperationException("Base VariableSet doesn't exist: " + baseVariableSetName);
        }

        var id = await _identityService.GetId<VariableSetId>();
        var variableSet = new VariableSetAggregate(id, variableSetName, baseVariableSetName);
        await _unitOfWork.VariableSets.AddAsync(variableSet);
        return variableSet;
    }

    public async Task<VariableSetComposed> GetVariableSetComposedAsync(
        string variableSetName,
        CancellationToken cancellationToken = default)
    {
        var aggregates = await _unitOfWork.VariableSets.FindAsync(new All(), cancellationToken);
        var sets = aggregates.Select(a => a.ToVariableSet());
        return VariableSetComposer.Compose(sets, variableSetName);
    }

    public async Task<VariableSetComposed> GetVariableSetComposedAsync(VariableSetId variableSetId,
        CancellationToken cancellationToken = default)
    {
        var variableSetName = (await _unitOfWork.VariableSets.GetAsync(variableSetId, cancellationToken)).VariableSetName;
        return await GetVariableSetComposedAsync(variableSetName, cancellationToken);
    }
}
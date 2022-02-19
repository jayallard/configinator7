using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;

namespace Allard.Configinator.Core.DomainServices;

public class VariableSetDomainService
{
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public VariableSetDomainService(IUnitOfWork unitOfWork, IIdentityService identityService,
        EnvironmentValidationService environmentValidationService)
    {
        _environmentValidationService =
            Guards.HasValue(environmentValidationService, nameof(environmentValidationService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
        _identityService = Guards.HasValue(identityService, nameof(identityService));
    }

    /// <summary>
    ///     Create a new VariableSet.
    /// </summary>
    /// <param name="variableSetName"></param>
    /// <param name="environmentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<VariableSetAggregate> CreateVariableSetAsync(
        string variableSetName,
        string environmentType,
        CancellationToken cancellationToken = default)
    {
        if (!_environmentValidationService.IsValidEnvironmentType(environmentType))
            throw new InvalidOperationException("Environment type doesn't exist: " + environmentType);

        var id = await _identityService.GetId<VariableSetId>();
        var variableSet = new VariableSetAggregate(id, null, null, variableSetName, environmentType);
        return variableSet;
    }

    /// <summary>
    ///     Create a new VariableSet, which is a child of an existing variable set.
    /// </summary>
    /// <param name="variableSetName"></param>
    /// <param name="baseVariableSetName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<VariableSetAggregate> CreateVariableSetOverride(
        string variableSetName,
        string baseVariableSetName)
    {
        // make sure the new name doesn't already exist
        if (await _unitOfWork.VariableSets.Exists(new VariableSetNameIs(variableSetName)))
            throw new InvalidOperationException("VariableSet already exists: " + variableSetName);

        var id = await _identityService.GetId<VariableSetId>();
        var baseVariableSet = await _unitOfWork.VariableSets.FindOneAsync(new VariableSetNameIs(baseVariableSetName));
        var child = new VariableSetAggregate(id, baseVariableSet.Id, baseVariableSet.VariableSetName, variableSetName,
            baseVariableSet.EnvironmentType);
        baseVariableSet.Play(new VariableSetOverrideCreatedEvent(id, baseVariableSet.Id));
        return child;
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
        var variableSetName =
            (await _unitOfWork.VariableSets.GetAsync(variableSetId, cancellationToken)).VariableSetName;
        return await GetVariableSetComposedAsync(variableSetName, cancellationToken);
    }
}
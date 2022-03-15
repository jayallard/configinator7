using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;

namespace Allard.Configinator.Core.DomainServices;

public class VariableSetDomainService
{
    private readonly EnvironmentDomainService _environmentDomainService;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public VariableSetDomainService(IUnitOfWork unitOfWork, IIdentityService identityService,
        EnvironmentDomainService environmentDomainService)
    {
        _environmentDomainService =
            Guards.HasValue(environmentDomainService, nameof(environmentDomainService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
        _identityService = Guards.HasValue(identityService, nameof(identityService));
    }

    /// <summary>
    ///     Create a new VariableSet.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="variableSetName"></param>
    /// <param name="environmentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<VariableSetAggregate> CreateVariableSetAsync(
        string @namespace,
        string variableSetName,
        string environmentType,
        CancellationToken cancellationToken = default)
    {
        if (!_environmentDomainService.EnvironmentTypeExists(environmentType))
            throw new InvalidOperationException("Environment type doesn't exist: " + environmentType);
        await EnsureVariableSetDoesntExistAsync(variableSetName, cancellationToken);
        var id = await _identityService.GetIdAsync<VariableSetId>(cancellationToken);
        var variableSet = new VariableSetAggregate(id, null, null, @namespace, variableSetName, environmentType);
        await _unitOfWork.VariableSets.AddAsync(variableSet, cancellationToken);
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
        string @namespace,
        string variableSetName,
        string baseVariableSetName,
        CancellationToken cancellationToken = default)
    {
        // make sure the new name doesn't already exist
        await EnsureVariableSetDoesntExistAsync(variableSetName, cancellationToken);
        var id = await _identityService.GetIdAsync<VariableSetId>(cancellationToken);
        var baseVariableSet = await _unitOfWork.VariableSets.FindOneAsync(new VariableSetNameIs(baseVariableSetName), cancellationToken);
        if (!NamespaceUtility.IsSelfOrAscendant(baseVariableSet.Namespace, @namespace))
            throw new InvalidOperationException("The base variable set must be in an self/ascendant namespace of the override." +
                                                $"\nVariable Set={baseVariableSet.Namespace}, {baseVariableSet.VariableSetName}" +
                                                $"\nOverride Set={@namespace}, {variableSetName}");

        var child = new VariableSetAggregate(id,
            baseVariableSet.Id,
            baseVariableSet.VariableSetName,
            @namespace,
            variableSetName,
            baseVariableSet.EnvironmentType);
        
        // TODO event driven
        baseVariableSet.AddOverride(id);
        await _unitOfWork.VariableSets.AddAsync(child, cancellationToken);
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
    
    private async Task EnsureVariableSetDoesntExistAsync(
        string variableSetName,
        CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.VariableSets.Exists(new VariableSetNameIs(variableSetName), cancellationToken);
        if (exists) throw new InvalidOperationException("The variable set name is already in use. Variable Set Name=" + variableSetName);
    }
}
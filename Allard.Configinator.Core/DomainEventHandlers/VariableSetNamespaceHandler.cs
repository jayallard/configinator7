using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainEventHandlers;

/// <summary>
///     When a variable set is created, add it to the namespace.
/// </summary>
public class VariableSetNamespaceHandler : IEventHandler<VariableSetCreatedEvent>
{
    private readonly NamespaceDomainService _namespaceDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public VariableSetNamespaceHandler(
        NamespaceDomainService namespaceDomainService,
        IUnitOfWork unitOfWork)
    {
        _namespaceDomainService = Guards.HasValue(namespaceDomainService, nameof(namespaceDomainService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task ExecuteAsync(VariableSetCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        var ns = await _namespaceDomainService.GetOrCreateAsync(evt.Namespace, cancellationToken);
        ns.AddVariableSet(evt.VariableSetId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainEventHandlers;

public class VariableSetNamespaceHandler : IEventHandler<VariableSetCreatedEvent>
{
    private readonly NamespaceDomainService _namespaceDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public VariableSetNamespaceHandler(
        NamespaceDomainService namespaceDomainService, 
        IUnitOfWork unitOfWork)
    {
        _namespaceDomainService = namespaceDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(VariableSetCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        var ns = await _namespaceDomainService.GetOrCreateAsync(evt.Namespace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
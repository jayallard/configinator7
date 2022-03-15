using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainEventHandlers;

/// <summary>
///     When a section is created, add it to the namespace.
/// </summary>
public class SectionNamespaceHandler : IDomainEventHandler<SectionCreatedEvent>
{
    private readonly NamespaceDomainService _namespaceDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public SectionNamespaceHandler(NamespaceDomainService namespaceDomainService, IUnitOfWork unitOfWork)
    {
        _namespaceDomainService = Guards.HasValue(namespaceDomainService, nameof(namespaceDomainService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task ExecuteAsync(SectionCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        var ns = await _namespaceDomainService.GetOrCreateAsync(evt.Namespace, cancellationToken);
        ns.AddSection(evt.SectionId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
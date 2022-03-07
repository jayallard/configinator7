using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainEventHandlers;

/// <summary>
///     When a schema is created, add it to the namespace.
/// </summary>
public class SchemaNamespaceHandler : IEventHandler<SchemaCreatedEvent>
{
    private readonly NamespaceDomainService _namespaceDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public SchemaNamespaceHandler(NamespaceDomainService namespaceDomainService, IUnitOfWork unitOfWork)
    {
        _namespaceDomainService = Guards.HasValue(namespaceDomainService, nameof(namespaceDomainService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task ExecuteAsync(SchemaCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        var ns = await _namespaceDomainService.GetOrCreateAsync(evt.Namespace, cancellationToken);
        ns.AddSchema(evt.SchemaId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
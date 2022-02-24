﻿using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainEventHandlers;

public class SchemaNamespaceHandler : IEventHandler<SchemaCreatedEvent>
{
    private readonly NamespaceDomainService _namespaceDomainService;
    private readonly IUnitOfWork _unitOfWork;
    public SchemaNamespaceHandler(NamespaceDomainService namespaceDomainService, IUnitOfWork unitOfWork)
    {
        _namespaceDomainService = namespaceDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(SchemaCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        var ns = await _namespaceDomainService.GetOrCreateAsync(evt.Namespace, cancellationToken);
        ns.AddSchema(evt.SchemaId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
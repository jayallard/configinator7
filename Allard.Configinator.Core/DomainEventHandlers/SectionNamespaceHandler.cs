﻿using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainEventHandlers;

public class SectionNamespaceHandler : IEventHandler<SectionCreatedEvent>
{
    private readonly NamespaceDomainService _namespaceDomainService;
    private readonly IUnitOfWork _unitOfWork;
    public SectionNamespaceHandler(NamespaceDomainService namespaceDomainService, IUnitOfWork unitOfWork)
    {
        _namespaceDomainService = namespaceDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(SectionCreatedEvent evt, CancellationToken cancellationToken = default)
    {
        var ns = await _namespaceDomainService.GetOrCreateAsync(evt.Namespace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
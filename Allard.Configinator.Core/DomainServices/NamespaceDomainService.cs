using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications.Namespaces;

namespace Allard.Configinator.Core.DomainServices;

public class NamespaceDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _idService;
    
    public NamespaceDomainService(IUnitOfWork unitOfWork, IIdentityService idService)
    {
        _unitOfWork = unitOfWork;
        _idService = idService;
    }

    public async Task<NamespaceAggregate> GetOrCreateAsync(string @namespace, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Namespaces.Exists(new NamespaceIs(@namespace), cancellationToken))
        {
            return await _unitOfWork.Namespaces.FindOneAsync(new NamespaceIs(@namespace), cancellationToken);
        }

        var id = await _idService.GetId<NamespaceId>();
        var ns = new NamespaceAggregate(id, @namespace);
        await _unitOfWork.Namespaces.AddAsync(ns, cancellationToken);
        return ns;
    } 
}
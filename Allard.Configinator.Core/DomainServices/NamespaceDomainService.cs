using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications.Namespaces;

namespace Allard.Configinator.Core.DomainServices;

public class NamespaceDomainService
{
    private readonly IIdentityService _idService;
    private readonly IUnitOfWork _unitOfWork;

    public NamespaceDomainService(IUnitOfWork unitOfWork, IIdentityService idService)
    {
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
        _idService = Guards.HasValue(idService, nameof(idService));
    }

    /// <summary>
    /// Get or create a namespace.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<NamespaceAggregate> GetOrCreateAsync(string @namespace,
        CancellationToken cancellationToken = default)
    {
        @namespace = NamespaceUtility.NormalizeNamespace(@namespace);
        if (await _unitOfWork.Namespaces.Exists(new NamespaceIs(@namespace), cancellationToken))
            return await _unitOfWork.Namespaces.FindOneAsync(new NamespaceIs(@namespace), cancellationToken);

        var id = await _idService.GetIdAsync<NamespaceId>(cancellationToken);
        var ns = new NamespaceAggregate(id, @namespace);
        await _unitOfWork.Namespaces.AddAsync(ns, cancellationToken);
        return ns;
    }
}
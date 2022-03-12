using System.Text.RegularExpressions;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications.Namespaces;

namespace Allard.Configinator.Core.DomainServices;

public class NamespaceDomainService
{
    private readonly IIdentityService _idService;
    private readonly IUnitOfWork _unitOfWork;
    public const string NamespaceValidatorPattern = @"^[a-zA-Z0-9_\.\-/]*$";
    private static readonly Regex NamespaceValidator = new Regex(NamespaceValidatorPattern, RegexOptions.Compiled);

    public NamespaceDomainService(IUnitOfWork unitOfWork, IIdentityService idService)
    {
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
        _idService = Guards.HasValue(idService, nameof(idService));
    }

    public static void EnsureValidNameSpace(string @namespace)
    {
        Guards.HasValue(@namespace, nameof(@namespace));
        if (@namespace == "/") return;
        if (!@namespace.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Namespaces must begin with /. Namespace=" + @namespace);
        }

        if (@namespace.EndsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Namespaces can't end with /. Namespace=" + @namespace);
        }

        if (@namespace.Contains("//", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Namespaces can't have empty segments. (//). Namespace=" + @namespace);
        }

        if (NamespaceValidator.IsMatch(@namespace)) return;
        throw new InvalidOperationException("Invalid namespace. It must match pattern: " + NamespaceValidatorPattern + ", Namespace=" + @namespace);
    }

    /// <summary>
    /// Get or create a namespace.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<NamespaceAggregate> GetOrCreateAsync(
        string @namespace,
        CancellationToken cancellationToken = default)
    {
        EnsureValidNameSpace(@namespace);
        if (await _unitOfWork.Namespaces.Exists(new NamespaceIs(@namespace), cancellationToken))
            return await _unitOfWork.Namespaces.FindOneAsync(new NamespaceIs(@namespace), cancellationToken);

        var id = await _idService.GetIdAsync<NamespaceId>(cancellationToken);
        var ns = new NamespaceAggregate(id, @namespace);
        await _unitOfWork.Namespaces.AddAsync(ns, cancellationToken);
        return ns;
    }
}
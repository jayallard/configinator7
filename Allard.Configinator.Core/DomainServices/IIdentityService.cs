using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainServices;

public interface IIdentityService
{
    Task<T> GetIdAsync<T>(CancellationToken cancellationToken = default) where T : IIdentity;
}
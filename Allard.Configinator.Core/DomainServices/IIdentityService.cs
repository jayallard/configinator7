using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.DomainServices;

public interface IIdentityService
{
    Task<T> GetId<T>() where T : IIdentity;
}
using Allard.Configinator.Core.DomainServices;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class IdentityServiceMemory : IIdentityService
{
    private readonly Dictionary<Type, long> _ids = new();

    public Task<T> GetIdAsync<T>(CancellationToken cancellationToken = default) where T : IIdentity
    {
        if (_ids.ContainsKey(typeof(T)))
        {
            var id = _ids[typeof(T)] + 1;
            _ids[typeof(T)] = id;
            return Task.FromResult(Create<T>(id));
        }

        _ids[typeof(T)] = 0;
        return Task.FromResult(Create<T>(0));
    }

    private static T Create<T>(long value)
    {
        return (T) Activator.CreateInstance(typeof(T), value)!;
    }
}
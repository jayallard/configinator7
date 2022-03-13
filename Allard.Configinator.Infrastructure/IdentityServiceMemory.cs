using System.Collections.Concurrent;
using Allard.Configinator.Core.DomainServices;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class IdentityServiceMemory : IIdentityService
{
    private readonly ConcurrentDictionary<Type, long> _ids = new();

    public Task<T> GetIdAsync<T>(CancellationToken cancellationToken = default) where T : IIdentity
    {
        var value = _ids.AddOrUpdate(typeof(T), 0, (id, count) => count + 1);
        var id = Create<T>(value);
        return Task.FromResult(id);
    }

    private static T Create<T>(long value) =>
        (T) Activator.CreateInstance(typeof(T), value)!;
}
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Repositories;

public interface IEventSourceRepository
{
    Task AppendAsync(Type type, long entityId, IEnumerable<ISourceEvent> events);
}
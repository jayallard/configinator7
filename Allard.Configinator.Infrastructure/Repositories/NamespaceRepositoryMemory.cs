using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure.Repositories;

public class NamespaceRepositoryMemory : RepositoryMemoryBase<NamespaceAggregate, NamespaceId>, INamespaceRepository
{
}
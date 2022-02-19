using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure.Repositories;

public class VariableSetRepositoryMemory : RepositoryMemoryBase<VariableSetAggregate, VariableSetId>,
    IVariableSetRepository
{
}
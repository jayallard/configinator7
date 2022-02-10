using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IVariableSetRepository : IRepository<VariableSetAggregate, VariableSetId>
{
}
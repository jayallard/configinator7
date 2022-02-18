using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure.Repositories;

public class GlobalSchemaRepositoryMemory :  RepositoryMemoryBase<GlobalSchemaAggregate, SchemaId>, IGlobalSchemaRepository
{
}
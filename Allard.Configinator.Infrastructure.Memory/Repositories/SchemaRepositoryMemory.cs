using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure.Memory.Repositories;

public class SchemaRepositoryMemory : RepositoryMemoryBase<SchemaAggregate, SchemaId>, ISchemaRepository
{
}
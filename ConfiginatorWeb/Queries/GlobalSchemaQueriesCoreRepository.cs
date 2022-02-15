using Allard.Configinator.Core;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Controllers;
using ConfiginatorWeb.Interactors;
using ConfiginatorWeb.Interactors.Section;

namespace ConfiginatorWeb.Queries;

public class GlobalSchemaQueriesCoreRepository : IGlobalSchemaQueries
{
    private readonly IGlobalSchemaRepository _repository;

    public GlobalSchemaQueriesCoreRepository(IGlobalSchemaRepository repository)
    {
        _repository = Guards.NotDefault(repository, nameof(repository));
    }

    public async Task<List<GlobalSchemaListItemDto>> GetGlobalSchemasListAsync(CancellationToken cancellationToken = default)
    {
        var schemas = await _repository.FindAsync(new All(), cancellationToken);
        return schemas.Select(gs => new GlobalSchemaListItemDto(gs.EntityId, gs.Name, gs.Description)).ToList();
    }
}
using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Configinator.Core.Specifications.Schema;
using Allard.DomainDrivenDesign;
using ConfiginatorWeb.Interactors.Queries.Section;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class SchemaQueriesCoreRepository : ISchemaQueries
{
    private readonly ISchemaRepository _repository;

    public SchemaQueriesCoreRepository(ISchemaRepository repository)
    {
        _repository = Guards.HasValue(repository, nameof(repository));
    }

    public Task<List<SchemaListItemDto>> GetSchemasListAsync(CancellationToken cancellationToken = default)
    {
        return Query(new All(), cancellationToken);
    }
    
    private async Task<List<SchemaListItemDto>> Query(ISpecification<SchemaAggregate> specification,
        CancellationToken cancellationToken)
    {
        var schemas = await _repository.FindAsync(specification, cancellationToken);
        return schemas
            .Select(gs =>
                new SchemaListItemDto(
                    gs.EntityId,
                    gs.SectionId?.Id,
                    gs.Namespace,
                    gs.SchemaName.ToOutputDto(),
                    gs.EnvironmentTypes.ToHashSet(),
                    gs.Description))
            .ToList();
    }
}
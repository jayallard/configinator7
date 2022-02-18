using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using NuGet.Versioning;

namespace Allard.Configinator.Core.DomainServices;

public class GlobalSchemaDomainService
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EnvironmentValidationService _environmentService;

    public GlobalSchemaDomainService(IIdentityService identityService, IUnitOfWork unitOfWork, EnvironmentValidationService environmentRules)
    {
        _environmentService = Guards.HasValue(environmentRules, nameof(environmentRules));
        _identityService = Guards.HasValue(identityService, nameof(identityService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task<GlobalSchemaAggregate> CreateGlobalSchemaAsync(
        string name, 
        string? description,
        JsonDocument schema)
    {
        SchemaName.Parse(name);
        if (await _unitOfWork.GlobalSchemas.Exists(new GlobalSchemaNameIs(name)))
        {
            throw new InvalidOperationException(
                $"Schema already exists: Name={name}");
        }
        
        var id = await _identityService.GetId<SchemaId>();
        var firstEnvironmentType = _environmentService.GetFirstEnvironmentType();
        return new GlobalSchemaAggregate(id, null, firstEnvironmentType, name, description, schema);
    }

    public async Task PromoteSchemaAsync(string name, string targetEnvironmentType,
        CancellationToken cancellationToken = default)
    {
        // hack - schemas need to be normalized across sections and global.
        // just make this work for now.
        var schema = await _unitOfWork.GlobalSchemas.FindOneAsync(new GlobalSchemaNameIs(name), cancellationToken);
        schema.Play(new GlobalSchemaPromotedEvent(schema.Id, targetEnvironmentType));
    }
}
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

    public GlobalSchemaDomainService(IIdentityService identityService, IUnitOfWork unitOfWork)
    {
        _identityService = Guards.NotDefault(identityService, nameof(identityService));
        _unitOfWork = Guards.NotDefault(unitOfWork, nameof(unitOfWork));
    }

    public async Task<GlobalSchemaAggregate> CreateGlobalSchemaAsync(
        string name, 
        string? description,
        JsonDocument schema)
    {
        if (await _unitOfWork.GlobalSchemas.Exists(new GlobalSchemaName(name)))
        {
            throw new InvalidOperationException(
                $"Schema already exists: Name={name}");
        }

        var id = await _identityService.GetId<GlobalSchemaId>();
        var schemaAggregate = new GlobalSchemaAggregate(id, name, description, schema);
        await _unitOfWork.GlobalSchemas.AddAsync(schemaAggregate);
        return schemaAggregate;
    }
}
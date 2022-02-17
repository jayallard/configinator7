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
        string environmentType,
        JsonDocument schema)
    {
        if (await _unitOfWork.GlobalSchemas.Exists(new GlobalSchemaName(name)))
        {
            throw new InvalidOperationException(
                $"Schema already exists: Name={name}");
        }

        if (!_environmentService.IsValidEnvironmentType(environmentType))
        {
            throw new InvalidOperationException("Invalid Environment Type: " + environmentType);
        }

        var id = await _identityService.GetId<GlobalSchemaId>();
        return new GlobalSchemaAggregate(id, environmentType, name, description, schema);
    }
}
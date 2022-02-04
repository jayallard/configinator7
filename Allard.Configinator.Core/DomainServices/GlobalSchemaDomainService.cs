using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using NJsonSchema;
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
        SemanticVersion version,
        JsonSchema schema)
    {
        if (await _unitOfWork.GlobalSchemas.Exists(new GetGlobalSchema(name, version)))
        {
            throw new InvalidOperationException(
                $"Schema already exists: Name={name}, Version={version.ToFullString()}");
        }

        var id = await _identityService.GetId<GlobalSchemaId>();
        return new GlobalSchemaAggregate(id, name, version, schema);
    }
}
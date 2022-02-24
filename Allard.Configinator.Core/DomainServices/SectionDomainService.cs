using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.DomainServices;

public class SectionDomainService
{
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly IIdentityService _identityService;
    private readonly SchemaLoader _schemaLoader;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VariableSetDomainService _variableSetDomainService;

    public SectionDomainService(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        VariableSetDomainService variableSetDomainService,
        SchemaLoader schemaLoader,
        EnvironmentValidationService environmentValidationService)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _variableSetDomainService = variableSetDomainService;
        _schemaLoader = schemaLoader;
        _environmentValidationService = environmentValidationService;
    }

    public async Task<SectionAggregate> CreateSectionAsync(string @namespace, string sectionName)
    {
        // make sure section doesn't already exist
        if (await _unitOfWork.Sections.Exists(sectionName))
            throw new InvalidOperationException("Section already exists: " + sectionName);

        var id = await _identityService.GetId<SectionId>();
        var section = new SectionAggregate(id, _environmentValidationService.GetFirstEnvironmentType(), @namespace, sectionName);
        return section;
    }

    public async Task<EnvironmentEntity> AddEnvironmentToSectionAsync(
        SectionAggregate section,
        string environmentName)
    {
        if (!_environmentValidationService.IsValidEnvironmentName(environmentName))
            throw new InvalidOperationException("Invalid environment name: " + environmentName);

        var environmentType = _environmentValidationService.GetEnvironmentType(environmentName);
        var id = await _identityService.GetId<EnvironmentId>();
        section.InternalEnvironments.EnsureEnvironmentDoesntExist(environmentName);
        section.PlayEvent(new EnvironmentCreatedEvent(id, section.Id, environmentType, environmentName));
        return section.GetEnvironment(environmentName);
    }

    public async Task<ReleaseEntity> CreateReleaseAsync(
        SectionAggregate section,
        EnvironmentId environmentId,
        VariableSetId? variableSetId,
        SchemaId schemaId,
        JsonDocument value,
        CancellationToken cancellationToken)
    {
        // get the variable set
        var variableSet = variableSetId == null
            ? null
            : await _variableSetDomainService.GetVariableSetComposedAsync(variableSetId, cancellationToken);

        // convert the value to a json.net value, which is needed for schema validation
        var jsonNetValue = value.ToJsonNetJson();

        // apply the variable replacements

        var jsonNetResolved = await JsonUtility.ResolveAsync(jsonNetValue,
            variableSet?.ToValueDictionary() ?? new Dictionary<string, JToken>(), cancellationToken);

        // validate against the schema
        var schema = await _unitOfWork.Schemas.GetAsync(schemaId, cancellationToken);
        var schemaDetails =
            await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema, cancellationToken);
        var validationErrors = schemaDetails.Root.ResolvedSchema!.Validate(jsonNetResolved);
        if (validationErrors.Any()) throw new SchemaValidationFailedException(validationErrors.ToList());

        // convert the json.net to system.text.json
        var resolved = jsonNetResolved.ToSystemTextJson();

        var releaseId = await _identityService.GetId<ReleaseId>();
        var variablesInUse = JsonUtility.GetVariableNames(jsonNetValue);
        var evt = new ReleaseCreatedEvent(
            releaseId,
            environmentId,
            section.Id,
            schemaId,
            variableSetId,
            value,
            resolved,
            variablesInUse);
        section.PlayEvent(evt);
        return section.GetRelease(environmentId, releaseId);
    }
}
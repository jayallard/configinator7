using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Core.Specifications;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Allard.Configinator.Core.DomainServices;

public class SectionDomainService
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VariableSetDomainService _variableSetDomainService;
    private readonly SchemaLoader _schemaLoader;
    private readonly EnvironmentValidationService _environmentValidationService;

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

    public async Task<SectionAggregate> CreateSectionAsync(string sectionName)
    {
        // make sure section doesn't already exist
        if (await _unitOfWork.Sections.Exists(sectionName))
        {
            throw new InvalidOperationException("Section already exists: " + sectionName);
        }

        var id = await _identityService.GetId<SectionId>();
        var section = new SectionAggregate(id, _environmentValidationService.GetFirstEnvironmentType(), sectionName);
        return section;
    }

    public async Task<SectionSchemaEntity> AddSchemaToSectionAsync(
        SectionAggregate section,
        string name,
        JsonDocument schema)
    {
        // make sure the schema is valid
        // this resolves the schema; confirms references are good.
        // resolves references from GlobalSchemaEntities,
        await _schemaLoader.ResolveSchemaAsync(schema);

        var id = await _identityService.GetId<SectionSchemaId>();
        return section.AddSchema(id, SchemaName.Parse(name), schema, _environmentValidationService.GetFirstEnvironmentType());
    }

    public async Task<SectionSchemaEntity> PromoteSchemaAsync(
        SectionAggregate section,
        string schemaName,
        string targetEnvironmentType,
        CancellationToken cancellationToken = default)
    {
        var schema = section.GetSchema(schemaName);
        var promotable = _environmentValidationService.CanPromoteTo(schema.EnvironmentTypes, targetEnvironmentType,
            SchemaName.Parse(schemaName));
        if (!promotable) throw new InvalidOperationException($"The schema cannot be promoted to {targetEnvironmentType}");
        section.PlayEvent(new SectionSchemaPromotedEvent(section.Id, schemaName, targetEnvironmentType));
        schema.InternalEnvironmentTypes.Add(targetEnvironmentType);
        return schema;
    }

    public async Task<EnvironmentEntity> AddEnvironmentToSectionAsync(
        SectionAggregate section,
        string environmentName)
    {
        if (!_environmentValidationService.IsValidEnvironmentName(environmentName))
        {
            throw new InvalidOperationException("Invalid environment name: " + environmentName);
        }

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
        SectionSchemaId sectionSchemaId,
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
        var schemaJson = section.GetSchema(sectionSchemaId).Schema;
        var schemaDetails = await _schemaLoader.ResolveSchemaAsync(schemaJson, cancellationToken);
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
            sectionSchemaId,
            variableSetId,
            value,
            resolved,
            variablesInUse);
        section.PlayEvent(evt);
        return section.GetRelease(environmentId, releaseId);
    }
}
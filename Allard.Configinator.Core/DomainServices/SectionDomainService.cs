using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Core.Specifications;
using Allard.Json;
using Newtonsoft.Json.Linq;

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
        JsonDocument schema,
        CancellationToken cancellationToken = default)
    {
        // make sure the schema is valid
        // this resolves the schema; confirms references are good.
        // resolves references from GlobalSchemaEntities,
        var resolved = await _schemaLoader.ResolveSchemaAsync(name, schema, cancellationToken);
        SchemaUtility.ValidateRootSchema(resolved.Root.ResolvedSchema, name);
        
        var sectionSchemaId = await _identityService.GetId<SectionSchemaId>();
        var firstEnvironmentType = _environmentValidationService.GetFirstEnvironmentType();
        section.InternalSchemas.EnsureDoesntExist(sectionSchemaId, name);
        section.PlayEvent(new SchemaAddedToSectionEvent(section.Id, sectionSchemaId, new SchemaName(name), schema, firstEnvironmentType));
        return section.GetSchema(sectionSchemaId);
    }

    public async Task<SectionSchemaEntity> PromoteSchemaAsync(
        SectionAggregate section,
        string schemaName,
        string targetEnvironmentType,
        CancellationToken cancellationToken = default)
    {
        var schema = section.GetSchema(schemaName);
        var isPromotable = _environmentValidationService.CanPromoteTo(
            schema.EnvironmentTypes, 
            targetEnvironmentType,
            SchemaName.Parse(schemaName));


        // make sure the promotion is allowed
        if (!isPromotable) throw new InvalidOperationException($"The schema cannot be promoted to {targetEnvironmentType}");

        // if any pre-release schemas are used, make sure pre-release is supported.
        var resolved = await _schemaLoader.ResolveSchemaAsync(schemaName, schema.Schema, cancellationToken);
        if (resolved.IsPreRelease && !_environmentValidationService.IsPreReleaseAllowed(targetEnvironmentType))
        {
            throw new InvalidOperationException("The environment type doesn't support pre-releases. EnvironmentType=" +
                                                targetEnvironmentType);
        }
        
        // make sure all references exist in the target environment type
        foreach (var r in resolved.References)
        {
            var global = await _unitOfWork.GlobalSchemas.FindOneAsync(new GlobalSchemaNameIs(r.Name.FullName), cancellationToken);
            if (!global.EnvironmentTypes.Contains(targetEnvironmentType))
            {
                throw new InvalidOperationException(
                    $"The schema, '{schemaName}', can't be promoted to '{targetEnvironmentType}'. It refers to '{global.Name}', which isn't assigned to '{targetEnvironmentType}'.");
            }            
        }
        
        
        section.PlayEvent(new SectionSchemaPromotedEvent(section.Id, schemaName, targetEnvironmentType));
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
        var schema = section.GetSchema(sectionSchemaId);
        var schemaDetails = await _schemaLoader.ResolveSchemaAsync(schema.SchemaName.FullName, schema.Schema, cancellationToken);
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
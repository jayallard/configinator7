using System.Text.Json;
using System.Xml.Serialization;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.DomainServices;

public class SectionDomainService
{
    private readonly EnvironmentDomainService _environmentDomainService;
    private readonly IIdentityService _identityService;
    private readonly SchemaLoader _schemaLoader;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VariableSetDomainService _variableSetDomainService;

    public SectionDomainService(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        VariableSetDomainService variableSetDomainService,
        SchemaLoader schemaLoader,
        EnvironmentDomainService environmentDomainService)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _variableSetDomainService = variableSetDomainService;
        _schemaLoader = schemaLoader;
        _environmentDomainService = environmentDomainService;
    }

    public async Task<SectionAggregate> CreateSectionAsync(string @namespace, string sectionName,
        CancellationToken cancellationToken = default)
    {
        // make sure section doesn't already exist
        if (await _unitOfWork.Sections.Exists(sectionName, cancellationToken))
            throw new InvalidOperationException("Section already exists: " + sectionName);

        var id = await _identityService.GetIdAsync<SectionId>(cancellationToken);
        var section = new SectionAggregate(id, _environmentDomainService.GetFirstEnvironmentType().EnvironmentTypeName, @namespace,
            sectionName);
        await _unitOfWork.Sections.AddAsync(section, cancellationToken);
        return section;
    }

    public Task PromoteToEnvironmentType(SectionAggregate section, string environmentType,
        CancellationToken cancellationToken = default)
    {
        _environmentDomainService.EnsureCanPromoteSectionTo(section.EnvironmentTypes, environmentType);
        section.PromoteTo(environmentType);
        return Task.CompletedTask;
    }

    public async Task<EnvironmentEntity> AddEnvironmentToSectionAsync(
        SectionAggregate section,
        string environmentName)
    {
        if (!_environmentDomainService.EnvironmentExists(environmentName))
            throw new InvalidOperationException("The environment doesn't exist: " + environmentName);

        section.Environments.EnsureEnvironmentDoesntExist(environmentName);
        var environmentType = _environmentDomainService.GetEnvironmentTypeForEnvironment(environmentName).EnvironmentTypeName;
        if (!section.EnvironmentTypes.Contains(environmentType))
            throw new InvalidOperationException($"The section doesn't support the {environmentType} environment type.");

        var environmentId = await _identityService.GetIdAsync<EnvironmentId>();
        return section.AddEnvironment(environmentId, environmentType, environmentName);
    }

    public async Task<ReleaseEntity> CreateReleaseAsync(
        SectionAggregate section,
        EnvironmentId environmentId,
        VariableSetId? variableSetId,
        SchemaId schemaId,
        JsonDocument value,
        CancellationToken cancellationToken = default)
    {
        // all good - create the release
        var resolvedValue = await ResolveValue(section.Id, environmentId, variableSetId, schemaId, value, cancellationToken);
        var releaseId = await _identityService.GetIdAsync<ReleaseId>(cancellationToken);
        return section.CreateRelease(releaseId, environmentId, variableSetId, schemaId, value, resolvedValue);
    }

    /// <summary>
    /// Convert the "value" to a resolved value.
    /// The value may contain variables. This will swap the variables for
    /// their values, then validate against the schema.
    /// If schema validation fails, an exception will be thrown.
    /// If all is well, it will return the resolved value.
    /// This is the method used by CreateRelease.
    /// It is PUBLIC so that the client can preview the json
    /// of a release rather than create the release.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="sectionId"></param>
    /// <param name="environmentId"></param>
    /// <param name="variableSetId"></param>
    /// <param name="schemaId"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="SchemaValidationFailedException"></exception>
    public async Task<JsonDocument> ResolveValue(
        SectionId sectionId,
        EnvironmentId environmentId,
        VariableSetId? variableSetId,
        SchemaId schemaId,
        JsonDocument value,
        CancellationToken cancellationToken = default)
    {
        // get the environment
        var section = await _unitOfWork.Sections.GetAsync(sectionId, cancellationToken);
        var env = section.GetEnvironment(environmentId);

        // make sure that the schema is available to the environment
        var schema = await _unitOfWork.Schemas.GetAsync(schemaId, cancellationToken);
        if (!schema.EnvironmentTypes.Contains(env.EnvironmentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"The schema doesn't belong to the release's environment type. Schema={schema.SchemaName.FullName}, Target Environment Type={env.EnvironmentType}");


        // convert the value to a json.net value, which is needed for schema validation
        var jsonNetValue = value.ToJsonNetJson();

        // get the variable set (if there is one)
        var variables = await GetAndValidateVariableSet(variableSetId, env, section, cancellationToken);

        // apply the variable replacements
        var jsonNetResolved = await JsonUtility.ResolveAsync(jsonNetValue,
            variables?.ToValueDictionary() ?? new Dictionary<string, JToken>(), cancellationToken);

        // get the schema and all of it's references
        var schemaDetails =
            await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema, cancellationToken);
        var validationErrors = schemaDetails.Root.ResolvedSchema!.Validate(jsonNetResolved);
        if (validationErrors.Any()) throw new SchemaValidationFailedException(jsonNetResolved.ToSystemTextJson(), validationErrors.ToList());

        // finally done
        // convert the json.net to system.text.json
        return jsonNetResolved.ToSystemTextJson();
    }

    private async Task<VariableSetComposed?> GetAndValidateVariableSet(
        VariableSetId? variableSetId,
        EnvironmentEntity env,
        SectionAggregate section,
        CancellationToken cancellationToken)
    {
        if (variableSetId == null) return null;
        var variableSet = await _unitOfWork.VariableSets.GetAsync(variableSetId, cancellationToken);

        // make sure the the variable set is the correct environment type
        if (!variableSet.EnvironmentType.Equals(env.EnvironmentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "The variable set isn't the same environment type as the release");

        // make sure the variable set namespace is valid for this section
        if (!NamespaceUtility.IsSelfOrAscendant(variableSet.Namespace, section.Namespace))
            throw new InvalidOperationException(
                "The variable set namespace must be at or above the section's namespace."
                + "\nVariable set Name = " + variableSet.VariableSetName
                + ",\nVariable Set Namespace = " + variableSet.Namespace
                + ",\nSection Namespace = " + section.Namespace);

        return await _variableSetDomainService.GetVariableSetComposedAsync(variableSetId, cancellationToken);
    }
}
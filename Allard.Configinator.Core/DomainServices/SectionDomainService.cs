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

    public SectionDomainService(IIdentityService identityService, IUnitOfWork unitOfWork,
        VariableSetDomainService variableSetDomainService, SchemaLoader schemaLoader)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _variableSetDomainService = variableSetDomainService;
        _schemaLoader = schemaLoader;
    }

    public async Task<SectionAggregate> CreateSectionAsync(string sectionName, string organizationPath)
    {
        // make sure section doesn't already exist
        if (await _unitOfWork.Sections.Exists(sectionName))
        {
            throw new InvalidOperationException("Section already exists: " + sectionName);
        }

        if (await _unitOfWork.Sections.Exists(new OrganizationPathIs(organizationPath)))
        {
            throw new InvalidOperationException("The organization path is already in use by another section");
        }

        var id = await _identityService.GetId<SectionId>();
        var section = new SectionAggregate(id, sectionName, organizationPath, null);
        await _unitOfWork.Sections.AddAsync(section);
        return section;
    }

    public async Task<SectionSchemaEntity> AddSchemaToSectionAsync(
        SectionAggregate section,
        SemanticVersion version,
        JsonDocument schema)
    {
        // make sure the schema is valid
        // this resolves the schema; confirms references are good.
        // resolves references from GlobalSchemaEntities,
        await _schemaLoader.ResolveSchemaAsync(schema);

        var id = await _identityService.GetId<SectionSchemaId>();
        return section.AddSchema(id, version, schema);
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
        var variablesInUse = JsonUtility.GetTokenNames(jsonNetValue);
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
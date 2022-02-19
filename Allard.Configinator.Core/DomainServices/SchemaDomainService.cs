using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Core.Specifications.Schema;

namespace Allard.Configinator.Core.DomainServices;

public class SchemaDomainService
{
    private readonly EnvironmentValidationService _environmentService;
    private readonly IIdentityService _identityService;
    private readonly SchemaLoader _schemaLoader;
    private readonly IUnitOfWork _unitOfWork;

    public SchemaDomainService(IIdentityService identityService, IUnitOfWork unitOfWork,
        EnvironmentValidationService environmentRules, SchemaLoader schemaLoader)
    {
        _schemaLoader = schemaLoader;
        _environmentService = Guards.HasValue(environmentRules, nameof(environmentRules));
        _identityService = Guards.HasValue(identityService, nameof(identityService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task<SchemaAggregate> CreateGlobalSchemaAsync(
        SchemaName name,
        string? description,
        JsonDocument schema)
    {
        if (await _unitOfWork.Schemas.Exists(SchemaNameIs.Is(name)))
            throw new InvalidOperationException(
                $"Schema already exists: Name={name}");

        var id = await _identityService.GetId<SchemaId>();
        var firstEnvironmentType = _environmentService.GetFirstEnvironmentType();
        return new SchemaAggregate(id, null, firstEnvironmentType, name, description, schema);
    }

    public async Task<SchemaAggregate> CreateSectionSchemaAsync(
        SchemaName schemaName,
        SectionId sectionId,
        string? description,
        JsonDocument schema,
        CancellationToken cancellationToken = default)
    {
        // schemas are globally unique
        var exists = await _unitOfWork.Schemas.Exists(SchemaNameIs.Is(schemaName));
        if (exists) throw new InvalidOperationException("That schema name is invalid. It is already in use.");

        var resolved = await _schemaLoader.ResolveSchemaAsync(schemaName, schema, cancellationToken);
        SchemaUtility.ValidateSectionReferences(resolved);
        SchemaUtility.ValidateRootSchema(resolved.Root.ResolvedSchema, schemaName.FullName);

        var section = await _unitOfWork.Sections.GetAsync(sectionId, cancellationToken);
        var schemaId = await _identityService.GetId<SchemaId>();
        var firstEnvironmentType = _environmentService.GetFirstEnvironmentType();
        var schemaAggregate = new SchemaAggregate(schemaId, sectionId, firstEnvironmentType, schemaName,
            description, schema);

        section.PlayEvent(new SchemaAddedToSectionEvent(section.Id, schemaId));
        return schemaAggregate;
    }
    
    public async Task<SchemaAggregate> PromoteSchemaAsync(SchemaName schemaName, string targetEnvironmentType,
        CancellationToken cancellationToken = default)
    {
        var schema = await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(schemaName), cancellationToken);
        var isPromotable = _environmentService.CanPromoteTo(
            schema.EnvironmentTypes,
            targetEnvironmentType,
            schema.SchemaName);


        // make sure the promotion is allowed
        if (!isPromotable)
            throw new InvalidOperationException($"The schema cannot be promoted to {targetEnvironmentType}");

        // if any pre-release schemas are used, make sure pre-release is supported.
        var resolved = await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema, cancellationToken);
        if (resolved.IsPreRelease && !_environmentService.IsPreReleaseAllowed(targetEnvironmentType))
            throw new InvalidOperationException("The environment type doesn't support pre-releases. EnvironmentType=" +
                                                targetEnvironmentType);

        // make sure all references exist in the target environment type
        foreach (var r in resolved.References)
        {
            var referencedSchema =
                await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(r.SchemaName),
                    cancellationToken);
            if (!referencedSchema.EnvironmentTypes.Contains(targetEnvironmentType))
                throw new InvalidOperationException(
                    $"The schema, '{schema.SchemaName.FullName}', can't be promoted to '{targetEnvironmentType}'. It refers to '{referencedSchema.SchemaName}', which isn't assigned to '{targetEnvironmentType}'.");
        }

        schema.Play(new GlobalSchemaPromotedEvent(schema.Id, targetEnvironmentType));
        return schema;
    }
}
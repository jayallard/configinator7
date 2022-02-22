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

    public async Task<SchemaAggregate> CreateSchemaAsync(
        SchemaName schemaName,
        SectionId? sectionId,
        string? description,
        JsonDocument schema,
        CancellationToken cancellationToken = default)
    {
        // schemas are globally unique
        // make sure this one doesn't already exist
        await EnsureSchemaDoesntExistAsync(schemaName, cancellationToken);

        // resolve the schema. This returns the schema and its references.
        // this is information only about the schemas and references; it has nothing to do with the
        // aggregates. the schema id isn't present, nor is anything 
        var resolved = await _schemaLoader.ResolveSchemaAsync(schemaName, schema, cancellationToken);

        // ---------------------------------------------------------------------------------------
        // make sure the schema has at least one property, etc.
        // the json value {} is a valid schema, but is useless.
        // this check makes sure that the schema is at least minimally useful
        // ---------------------------------------------------------------------------------------
        SchemaUtility.ValidateRootSchema(resolved.Root.ResolvedSchema, schemaName.FullName);

        // ---------------------------------------------------------------------------------------
        // make sure that all schemas are either global, or are in the same section.
        // ---------------------------------------------------------------------------------------
        var validationProperties = await GetSchemaValidationProperties(schemaName, sectionId, resolved.References, cancellationToken);
        SchemaUtility.ValidateSchemasGroup(validationProperties, sectionId);

        // ---------------------------------------------------------------------------------------
        // All good. create the schema.
        // ---------------------------------------------------------------------------------------
        return await ReallyCreateSchemaAsync(schemaName, sectionId, description, schema, cancellationToken);
    }

    private async Task<IEnumerable<SchemaValidationProperties>> GetSchemaValidationProperties(SchemaName schemaName,
        SectionId sectionId,
        IEnumerable<SchemaDetail> resolved,
        CancellationToken cancellationToken)
    {
        // get all references from the db
        var reference = await GetSchemasAsync(resolved.Select(r => r.SchemaName), cancellationToken);
        
        // convert the references to SchemaValidationProperties
        var validationInfo = reference
            .Select(r => new SchemaValidationProperties(r.SchemaName, r.SectionId))
            
            // add properties for the new schema that we're trying to create.
            .Union(new[] {new SchemaValidationProperties(schemaName, sectionId)});
        return validationInfo;
    }

    /// <summary>
    /// Actually create the schema.
    /// The schema is known to be valid by the time this is called.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="sectionId"></param>
    /// <param name="description"></param>
    /// <param name="schema"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<SchemaAggregate> ReallyCreateSchemaAsync(
        SchemaName schemaName,
        SectionId? sectionId,
        string? description,
        JsonDocument schema, CancellationToken cancellationToken)
    {
        var schemaId = await _identityService.GetId<SchemaId>();
        var firstEnvironmentType = _environmentService.GetFirstEnvironmentType();
        var schemaAggregate = new SchemaAggregate(schemaId, sectionId, firstEnvironmentType, schemaName,
            description, schema);

        if (sectionId == null) return schemaAggregate;

        // TODO: this should be event driven.
        var section = await _unitOfWork.Sections.GetAsync(sectionId, cancellationToken);
        section.PlayEvent(new SchemaAddedToSectionEvent(section.Id, schemaId));
        return schemaAggregate;
    }

    /// <summary>
    /// Throws an exception if the schema doesn't exist.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task EnsureSchemaDoesntExistAsync(SchemaName schemaName,
        CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Schemas.Exists(SchemaNameIs.Is(schemaName), cancellationToken);
        if (exists) throw new InvalidOperationException("That schema name is invalid. It is already in use.");
    }

    public async Task<SchemaAggregate[]> GetSchemasAsync(IEnumerable<SchemaName> schemaNames,
        CancellationToken cancellationToken = default)
    {
        // now, get all of the schema aggregates for the schemas in use.
        var allUsedSchemaTasks = schemaNames
            // todo: convert to a FIND with all of the names
            .Select(async name => await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(name), cancellationToken))
            .ToArray();

        await Task.WhenAll(allUsedSchemaTasks);
        var allUsedSchemas = allUsedSchemaTasks
            .Select(s => s.Result)
            .ToArray();
        return allUsedSchemas;
    }

    public async Task<SchemaAggregate> PromoteSchemaAsync(
        SchemaName schemaName, 
        string targetEnvironmentType,
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
        if (resolved.IsPreRelease() && !_environmentService.IsPreReleaseAllowed(targetEnvironmentType))
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

        schema.Play(new SchemaPromotedEvent(schema.Id, targetEnvironmentType));
        return schema;
    }
}
using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Core.Specifications.Schema;

namespace Allard.Configinator.Core.DomainServices;

public class SchemaDomainService
{
    private readonly EnvironmentDomainService _environmentService;
    private readonly IIdentityService _identityService;
    private readonly SchemaLoader _schemaLoader;
    private readonly IUnitOfWork _unitOfWork;

    public SchemaDomainService(IIdentityService identityService, IUnitOfWork unitOfWork,
        EnvironmentDomainService environmentRules, SchemaLoader schemaLoader)
    {
        _schemaLoader = Guards.HasValue(schemaLoader, nameof(schemaLoader));
        _environmentService = Guards.HasValue(environmentRules, nameof(environmentRules));
        _identityService = Guards.HasValue(identityService, nameof(identityService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    /// <summary>
    ///     Create a schema.
    /// </summary>
    /// <param name="sectionId">If specified, the schema is for the configuration section.</param>
    /// <param name="namespace"></param>
    /// <param name="schemaName"></param>
    /// <param name="description"></param>
    /// <param name="schema"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<SchemaAggregate> CreateSchemaAsync(
        SectionId? sectionId,
        string @namespace,
        SchemaName schemaName,
        string? description,
        JsonDocument schema,
        CancellationToken cancellationToken = default)
    {
        // schemas are globally unique
        // make sure this one doesn't already exist
        await EnsureSchemaDoesntExistAsync(schemaName, cancellationToken);

        // ---------------------------------------------------------------------------------------
        // if the schema is for a section, make sure they're in the same namespace.
        // ---------------------------------------------------------------------------------------
        if (sectionId != null)
        {
            var section = await _unitOfWork.Sections.GetAsync(sectionId, cancellationToken);
            if (!section.Namespace.Equals(@namespace, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The schema must be in the same namespace as the section.\n" +
                                                    "Section Namespace=" + section.Namespace +
                                                    ",\nSchema Namespace=" + @namespace +
                                                    ",\nSchema Name=" + schemaName.FullName);
        }

        // ---------------------------------------------------------------------------------------
        // if it's a prerelease, make sure prerelease is supported
        // ---------------------------------------------------------------------------------------
        if (schemaName.Version.IsPrerelease)
        {
            var firstEnvironmentType = _environmentService.GetFirstEnvironmentType();
            if (!firstEnvironmentType.SupportsPreRelease)
            {
                throw new InvalidOperationException("PreRelease isn't supported. Environment Type: " +
                                                    firstEnvironmentType.EnvironmentTypeName);
            }
        }


        // resolve the json schema. This returns raw data about the schema and it's references.
        // it is not aggregate aware.
        var resolved = await _schemaLoader.ResolveSchemaAsync(schemaName, schema, cancellationToken);

        // ---------------------------------------------------------------------------------------
        // make sure the schema has at least one property, etc.
        // the json value {} is a valid schema, but is useless.
        // this check makes sure that the schema is at least minimally useful
        // ---------------------------------------------------------------------------------------
        SchemaUtility.EnsureValidSchema(resolved.Root.ResolvedSchema, schemaName.FullName);

        // ---------------------------------------------------------------------------------------
        // make sure that all schemas are in valid namespaces
        // ---------------------------------------------------------------------------------------
        var schemaProperties = new SchemaValidationProperties(@namespace, schemaName);
        var referenceProperties =
            await GetReferenceValidationProperties(resolved.References, cancellationToken);
        SchemaUtility.ValidateSchemaNamespaces(schemaProperties, referenceProperties);

        // ---------------------------------------------------------------------------------------
        // All good. create the schema.
        // ---------------------------------------------------------------------------------------
        return await ReallyCreateSchemaAsync(sectionId, @namespace, schemaName, description, schema, cancellationToken);
    }

    private async Task<IEnumerable<SchemaValidationProperties>> GetReferenceValidationProperties(
        IEnumerable<SchemaDetail> schemas,
        CancellationToken cancellationToken)
    {
        // get all references from the db
        var reference = await GetSchemasAsync(schemas.Select(r => r.SchemaName), cancellationToken);

        // convert the references to SchemaValidationProperties
        var validationInfo = reference
            .Select(r => new SchemaValidationProperties(r.Namespace, r.SchemaName));
        return validationInfo;
    }

    /// <summary>
    ///     Actually create the schema.
    ///     The schema is known to be valid by the time this is called.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="schemaName"></param>
    /// <param name="sectionId"></param>
    /// <param name="description"></param>
    /// <param name="schema"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<SchemaAggregate> ReallyCreateSchemaAsync(
        SectionId? sectionId,
        string @namespace,
        SchemaName schemaName,
        string? description,
        JsonDocument schema, CancellationToken cancellationToken)
    {
        var schemaId = await _identityService.GetIdAsync<SchemaId>(cancellationToken);
        var firstEnvironmentType = _environmentService.GetFirstEnvironmentType().EnvironmentTypeName;
        var schemaAggregate = new SchemaAggregate(
            schemaId,
            sectionId,
            firstEnvironmentType,
            @namespace,
            schemaName,
            description,
            schema);
        await _unitOfWork.Schemas.AddAsync(schemaAggregate, cancellationToken);
        return schemaAggregate;
    }

    /// <summary>
    ///     Throws an exception if the schema doesn't exist.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task EnsureSchemaDoesntExistAsync(SchemaName schemaName,
        CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Schemas.Exists(SchemaNameIs.Is(schemaName), cancellationToken);
        if (exists) throw new InvalidOperationException("The schema name is already in use. SchemaName=" + schemaName.FullName);
    }

    /// <summary>
    ///     Get 0 or more schemas.
    /// </summary>
    /// <param name="schemaNames"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SchemaAggregate[]> GetSchemasAsync(IEnumerable<SchemaName> schemaNames,
        CancellationToken cancellationToken = default)
    {
        var s = schemaNames.ToArray();
        
        // now, get all of the schema aggregates for the schemas in use.
        var allUsedSchemaTasks = s
            // todo: convert to a FIND with all of the names
            .Select(async name => await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(name), cancellationToken))
            .ToArray();

        await Task.WhenAll(allUsedSchemaTasks);
        var allUsedSchemas = allUsedSchemaTasks
            .Select(s => s.Result)
            .ToArray();
        if (s.Length != allUsedSchemas.Length)
        {
            throw new InvalidOperationException("1 or more of the schemas doesn't exist.");
        }
        
        return allUsedSchemas;
    }

    /// <summary>
    ///     Promote a schema from one environment type to another.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="targetEnvironmentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task PromoteSchemaAsync(
        SchemaName schemaName,
        string targetEnvironmentType,
        CancellationToken cancellationToken = default)
    {
        var schema = await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(schemaName), cancellationToken);

        // make sure the promotion is allowed
        _environmentService.EnsureCanPromoteSchemaTo(
            schema.EnvironmentTypes,
            targetEnvironmentType,
            schema.SchemaName);

        // if any pre-release schemas are used, make sure pre-release is supported.
        var resolved = await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema, cancellationToken);

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

        schema.Promote(targetEnvironmentType);
    }
}
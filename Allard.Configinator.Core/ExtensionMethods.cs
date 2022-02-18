using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Allard.Configinator.Core;

public static class ExtensionMethods
{
    public static IDictionary<string, JToken> ToValueDictionary(this VariableSetComposed variables) =>
        variables
            .VariablesResolved
            .ToDictionary(
                t => t.Key,
                t => t.Value.Value,
                StringComparer.OrdinalIgnoreCase);

    private static void EnsureIdDoesntExist<TIdentity>(this IEnumerable<IEntity> entities, TIdentity id,
        string parameterName)
        where TIdentity : IIdentity
    {
        if (!entities.Any(e => e.EntityId.Equals(id.Id))) return;
        throw new InvalidOperationException($"{parameterName} already exists. Id={id.Id}");
    }

    private static void EnsureIdExists<TIdentity>(this IEnumerable<IEntity> entities, TIdentity id,
        string parameterName)
        where TIdentity : IIdentity
    {
        if (entities.Any(e => e.EntityId.Equals(id.Id))) return;
        throw new InvalidOperationException($"{parameterName} doesn't exists. Id={id.Id}");
    }

    public static void EnsureReleaseDoesntExist(this IEnumerable<ReleaseEntity> releases, ReleaseId id)
        => releases.EnsureIdDoesntExist(id, "Release");

    public static void EnsureExists(this IEnumerable<ReleaseEntity> releases, ReleaseId id)
        => releases.EnsureIdExists(id, "Release");

    public static void EnsureDeploymentDoesntExist(this IEnumerable<DeploymentEntity> deployments,
        DeploymentId id)
        => deployments.EnsureIdDoesntExist(id, "DeploymentHistory");

    public static void EnsureDoesntExist(this IEnumerable<SectionSchemaEntity> schemas, SectionSchemaId id,
        string? name = null)
    {
        var schemaEntities = schemas as SectionSchemaEntity[] ?? schemas.ToArray();
        schemaEntities.EnsureIdDoesntExist(id, "Schema");
        // TODO: don't need fullname
        if (name != null && schemaEntities.Any(s => s.SchemaName.FullName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Schema already exists. Name=" + name);
    }

    public static void EnsureEnvironmentDoesntExist(this IEnumerable<EnvironmentEntity> environments,
        string environmentName)
    {
        if (environments.Any(e => e.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Environment already exists. Name=" + environmentName);
    }

    public static void EnsureEnvironmentExists(this IEnumerable<EnvironmentEntity> environments, EnvironmentId id) =>
        environments.EnsureIdExists(id, "Environment");

    /// <summary>
    /// Get an entity from a list of entities, by id.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TIdentity"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static TEntity GetEntity<TEntity, TIdentity>(this IEnumerable<TEntity> entities, TIdentity id,
        string name) where TIdentity : IIdentity
        where TEntity : IEntity
    {
        var entity = entities.SingleOrDefault(e => e.EntityId.Equals(id.Id));
        if (entity == null) throw new InvalidOperationException($"{name} doesn't exist. Id={id.Id}");
        return entity;
    }

    public static EnvironmentEntity GetEnvironment(this IEnumerable<EnvironmentEntity> entities, EnvironmentId id)
        => entities.GetEntity(id, "Entity");

    public static ReleaseEntity GetRelease(
        this IEnumerable<ReleaseEntity> entities,
        ReleaseId id)
        => entities.GetEntity(id, "Release");

    public static DeploymentEntity GetDeployment(
        this IEnumerable<DeploymentEntity> entities,
        DeploymentId id)
        => entities.GetEntity(id, "Deployment");
}
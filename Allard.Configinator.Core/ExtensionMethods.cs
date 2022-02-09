using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Allard.Configinator.Core;

public static class ExtensionMethods
{
    public static IDictionary<string, JToken> ToValueDictionary(this TokenSetComposed3 tokens) =>
        tokens
            .Tokens
            .ToDictionary(
                t => t.Key, 
                t => t.Value, 
                StringComparer.OrdinalIgnoreCase);

    private static void EnsureDoesntExist<TIdentity>(this IEnumerable<IEntity> entities, TIdentity id,
        string parameterName)
        where TIdentity : IIdentity
    {
        if (!entities.Any(e => e.EntityId.Equals(id.Id))) return;
        throw new InvalidOperationException($"{parameterName} already exists. Id={id.Id}");
    }

    private static void EnsureExists<TIdentity>(this IEnumerable<IEntity> entities, TIdentity id,
        string parameterName)
        where TIdentity : IIdentity
    {
        if (entities.Any(e => e.EntityId.Equals(id.Id))) return;
        throw new InvalidOperationException($"{parameterName} doesn't exists. Id={id.Id}");
    }

    public static void EnsureReleaseDoesntExist(this IEnumerable<ReleaseEntity> releases, ReleaseId id)
        => releases.EnsureDoesntExist(id, "Release");

    public static void EnsureExists(this IEnumerable<ReleaseEntity> releases, ReleaseId id)
        => releases.EnsureExists(id, "Release");

    public static void EnsureDeploymentDoesntExist(this IEnumerable<DeploymentEntity> deployments,
        DeploymentId id)
        => deployments.EnsureDoesntExist(id, "DeploymentHistory");

    public static void EnsureDoesntExist(this IEnumerable<SectionSchemaEntity> schemas, SectionSchemaId id,
        SemanticVersion? version = null)
    {
        var schemaEntities = schemas as SectionSchemaEntity[] ?? schemas.ToArray();
        schemaEntities.EnsureDoesntExist(id, "Schema");
        if (version != null && schemaEntities.Any(s => s.Version.Equals(version)))
            throw new InvalidOperationException("Schema already exists. Version=" + version.ToFullString());
    }

    public static void EnsureEnvironmentDoesntExist(this IEnumerable<EnvironmentEntity> environments, EnvironmentId id,
        string? name = null)
    {
        var environmentEntities = environments as EnvironmentEntity[] ?? environments.ToArray();
        environmentEntities.EnsureDoesntExist(id, "Environment");
        if (name != null &&
            environmentEntities.Any(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Environment already exists. Name=" + name);
    }

    public static void EnsureEnvironmentExists(this IEnumerable<EnvironmentEntity> environments, EnvironmentId id) =>
        environments.EnsureExists(id, "Environment");

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
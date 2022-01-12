using Allard.Configinator.Core.Model;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Allard.Configinator.Core;

public static class ExtensionMethods
{
    public static IDictionary<string, JToken> ToValueDictionary(this TokenSetComposed tokens) =>
        tokens
            .Tokens
            .Values
            .ToDictionary(t => t.Name, t => t.Token, StringComparer.OrdinalIgnoreCase);

    private static void EnsureDoesntExist<TIdentity>(this IEnumerable<IEntity<TIdentity>> entities, TIdentity id,
        string parameterName)
        where TIdentity : IIdentity
    {
        if (!entities.Any(e => e.Id.Equals(id))) return;
        throw new InvalidOperationException($"{parameterName} already exists. Id={id.Id}");
    }

    private static void EnsureExists<TIdentity>(this IEnumerable<IEntity<TIdentity>> entities, TIdentity id,
        string parameterName)
        where TIdentity : IIdentity
    {
        if (entities.Any(e => e.Id.Equals(id))) return;
        throw new InvalidOperationException($"{parameterName} doesn't exists. Id={id.Id}");
    }

    public static void EnsureReleaseDoesntExist(this IEnumerable<ReleaseEntity> releases, ReleaseId id)
        => releases.EnsureDoesntExist(id, "Release");

    public static void EnsureExists(this IEnumerable<ReleaseEntity> releases, ReleaseId id)
        => releases.EnsureExists(id, "Release");

    public static void EnsureDeploymentDoesntExist(this IEnumerable<DeploymentHistoryEntity> deployments,
        DeploymentHistoryId id)
        => deployments.EnsureDoesntExist(id, "DeploymentHistory");

    public static void EnsureDoesntExist(this IEnumerable<SchemaEntity> schemas, SchemaId id,
        SemanticVersion? version = null)
    {
        var schemaEntities = schemas as SchemaEntity[] ?? schemas.ToArray();
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

    private static TEntity GetEntity<TEntity, TIdentity>(this IEnumerable<TEntity> entities, TIdentity id,
        string name) where TIdentity : IIdentity
        where TEntity : IEntity<TIdentity>
    {
        var entity = entities.SingleOrDefault(e => e.Id.Equals(id));
        if (entity == null) throw new InvalidOperationException($"{name} doesn't exist. Id={id.Id}");
        return entity;
    }

    public static EnvironmentEntity GetEnvironment(this IEnumerable<EnvironmentEntity> entities, EnvironmentId id)
        => entities.GetEntity(id, "Entity");

    public static ReleaseEntity GetRelease(
        this IEnumerable<ReleaseEntity> entities,
        ReleaseId id)
        => entities.GetEntity(id, "Release");

    public static DeploymentHistoryEntity GetDeployment(
        this IEnumerable<DeploymentHistoryEntity> entities,
        DeploymentHistoryId id)
        => entities.GetEntity(id, "Deployment");
}
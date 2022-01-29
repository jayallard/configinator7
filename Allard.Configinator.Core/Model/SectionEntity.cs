using System.Text.Json;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionEntity : AggregateBase<SectionId>
{
    internal List<SchemaEntity> InternalSchemas { get; } = new();
    internal List<EnvironmentEntity> InternalEnvironments { get; } = new();
    public IEnumerable<SchemaEntity> Schemas => InternalSchemas.AsReadOnly();
    public IEnumerable<EnvironmentEntity> Environments => InternalEnvironments.AsReadOnly();
    public SectionId Id { get; internal set; }
    public string SectionName { get; internal set; }
    public string Path { get; internal set; }
    public string? TokenSetName { get; internal set; }

    public EnvironmentEntity GetEnvironment(string name) =>
        InternalEnvironments.Single(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase));

    public EnvironmentEntity GetEnvironment(EnvironmentId environmentId) =>
        InternalEnvironments.GetEnvironment(environmentId);


    internal SectionEntity(SectionId id, string name, string path, SchemaEntity? schema = null,
        string? tokenSetName = null) : base(id)
    {
        Guards.NotDefault(id, nameof(id));
        Guards.NotEmpty(path, nameof(name));
        Guards.NotEmpty(path, nameof(path));
        PlayEvent(new SectionCreatedEvent(id, name, path, schema, tokenSetName));
    }

    public SectionEntity(IEnumerable<IDomainEvent> events) : base(new SectionId(3))
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) PlayEvent(evt);
        InternalSourceEvents.Clear();
    }

    internal void PlayEvent(IDomainEvent evt)
    {
        SectionAggregateEventHandlers.Play(this, evt);
        InternalSourceEvents.Add(evt);
    }

    public SchemaEntity AddSchema(SchemaId schemaId, SemanticVersion schemaVersion, JsonSchema schema)
    {
        InternalSchemas.EnsureDoesntExist(schemaId, schemaVersion);
        PlayEvent(new SchemaAddedToSectionEvent(Id, schemaId, schemaVersion, schema));
        return GetSchema(schemaId);
    }

    public SchemaEntity GetSchema(SchemaId schemaId) =>
        InternalSchemas.Single(s => s.Id == schemaId);

    public EnvironmentEntity AddEnvironment(EnvironmentId environmentId, string name)
    {
        InternalEnvironments.EnsureEnvironmentDoesntExist(environmentId, name);
        PlayEvent(new EnvironmentCreatedEvent(environmentId, Id, name));
        return GetEnvironment(name);
    }

    public ReleaseEntity GetRelease(EnvironmentId environmentId, ReleaseId releaseId) =>
        GetEnvironment(environmentId).InternalReleases.GetRelease(releaseId);

    public async Task<ReleaseEntity> CreateReleaseAsync(
        EnvironmentId environmentId,
        ReleaseId releaseId,
        TokenSetComposed? tokens,
        SchemaId schemaId,
        JsonDocument value,
        CancellationToken cancellationToken = default)
    {
        var env = GetEnvironment(environmentId);
        env.InternalReleases.EnsureReleaseDoesntExist(releaseId);
        var schema = GetSchema(schemaId);
        var tokenValues = tokens?.ToValueDictionary() ?? new Dictionary<string, JToken>();

        // System.Text.Json is immutable, which we like.
        // - NJsonSchema requires newtonsoft.
        // - Resolve requires mutable objects.
        // so, using System.Text.Json as much as possible.
        // but here we need to convert to JsonNet, then back.
        var newtonValue = value.ToJsonNetJson();
        var newtonResolvedValue = await JsonUtility.ResolveAsync(newtonValue, tokenValues, cancellationToken);

        ValidateAgainstSchema(newtonResolvedValue, schema.Schema);
        var resolvedValue = newtonResolvedValue.ToSystemTextJson();
        var tokensInUse = JsonUtility.GetTokenNamesDeep(newtonValue, tokenValues).ToHashSet();
        var evt = new ReleaseCreatedEvent(
            releaseId,
            env.Id,
            Id,
            schemaId,
            value,
            resolvedValue,
            tokens,
            tokensInUse);
        PlayEvent(evt);
        return GetRelease(environmentId, releaseId);
    }

    public DeploymentEntity SetDeployed(
        EnvironmentId environmentId,
        ReleaseId releaseId,
        DeploymentId deploymentId,
        DateTime deploymentDate)
    {
        var release = GetRelease(environmentId, releaseId);
        release.InternalDeployments.EnsureDeploymentDoesntExist(deploymentId);

        // if an active deployment exists, remove it
        SetActiveDeploymentToRemoved(environmentId, releaseId, deploymentId);

        // create the new deployment.
        var deployedEvt = new ReleaseDeployedEvent(
            deploymentId,
            deploymentDate,
            Id,
            environmentId,
            releaseId);
        PlayEvent(deployedEvt);
        return GetDeployment(environmentId, releaseId, deploymentId);
    }

    public DeploymentEntity GetDeployment(
        EnvironmentId environmentId,
        ReleaseId releaseId,
        DeploymentId deploymentId)
    {
        return GetRelease(environmentId, releaseId)
            .InternalDeployments
            .GetDeployment(deploymentId);
    }


    private static void ValidateAgainstSchema(JToken value, JsonSchema schema)
    {
        var results = schema.Validate(value);
        if (results.Any())
        {
            throw new SchemaValidationFailedException(results);
        }
    }

    /// <summary>
    /// When deploying: see if any other release for the environment is already
    /// deployed. If so, remove it.
    /// </summary>
    /// <param name="environmentId"></param>
    /// <param name="releaseId"></param>
    /// <param name="deploymentId"></param>
    private void SetActiveDeploymentToRemoved(EnvironmentId environmentId, ReleaseId releaseId,
        DeploymentId deploymentId)
    {
        // see if any deployment for any release in the environment is
        // currently deployed.
        // if so, set it to removed.
        var deployed = GetEnvironment(environmentId)
            .Releases
            .SelectMany(r => r.Deployments
                .Where(d => d.Id != deploymentId)
                .Where(d => d.IsDeployed)
                .Select(d => new {Release = r, Deployment = d})
            )
            .SingleOrDefault();
        
        if (deployed is null) return;
        var removedEvent = new DeploymentRemovedEvent(
            deployed.Deployment.Id,
            Id,
            environmentId,
            deployed.Release.Id,
            $"Replaced by ReleaseId={deployed.Release.Id.Id}, Deployment Id={deploymentId.Id}");
        PlayEvent(removedEvent);
    }
}
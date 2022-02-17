using System.Text.Json;
using Allard.DomainDrivenDesign;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionAggregate : AggregateBase<SectionId>
{
    internal ISet<string> InternalEnvironmentTypes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    internal List<SectionSchemaEntity> InternalSchemas { get; } = new();
    internal List<EnvironmentEntity> InternalEnvironments { get; } = new();
    public IEnumerable<SectionSchemaEntity> Schemas => InternalSchemas.AsReadOnly();
    public IEnumerable<EnvironmentEntity> Environments => InternalEnvironments.AsReadOnly();
    public string SectionName { get; internal set; }
    public string OrganizationPath { get; internal set; }
    public EnvironmentEntity GetEnvironment(string name) =>
        InternalEnvironments.Single(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase));
    public EnvironmentEntity GetEnvironment(EnvironmentId environmentId) =>
        InternalEnvironments.GetEnvironment(environmentId);

    /// <summary>
    /// Gets the environment types that can host this section.
    /// </summary>
    public IEnumerable<string> EnvironmentTypes => InternalEnvironmentTypes.ToList();
    internal SectionAggregate(SectionId id, string initialEnvironmentType, string name, string organizationPath)
    {
        Guards.HasValue(id, nameof(id));
        Guards.HasValue(organizationPath, nameof(name));
        Guards.HasValue(organizationPath, nameof(organizationPath));
        Guards.HasValue(initialEnvironmentType, nameof(initialEnvironmentType));
        PlayEvent(new SectionCreatedEvent(id, name, initialEnvironmentType, organizationPath));
    }

    internal SectionAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) PlayEvent(evt);
        InternalSourceEvents.Clear();
    }

    internal void PlayEvent(IDomainEvent evt)
    {
        SectionAggregateEventHandlers.Play(this, evt);
        InternalSourceEvents.Add(evt);
    }

    internal SectionSchemaEntity AddSchema(
        SectionSchemaId sectionSchemaId, 
        SemanticVersion schemaVersion, 
        JsonDocument schema,
        string environmentType)
    {
        InternalSchemas.EnsureDoesntExist(sectionSchemaId, schemaVersion);
        PlayEvent(new SchemaAddedToSectionEvent(Id, sectionSchemaId, schemaVersion, schema, environmentType));
        return GetSchema(sectionSchemaId);
    }

    public SectionSchemaEntity GetSchema(SectionSchemaId sectionSchemaId) =>
        InternalSchemas.Single(s => s.Id == sectionSchemaId);

    public SectionSchemaEntity GetSchema(SemanticVersion version) =>
        InternalSchemas.Single(s => s.Version == version);
    
    public ReleaseEntity GetRelease(EnvironmentId environmentId, ReleaseId releaseId) =>
        GetEnvironment(environmentId).InternalReleases.GetRelease(releaseId);
    
    public DeploymentResult? DeploymentResult { get; private set; }

    public DeploymentEntity SetDeployed(
        EnvironmentId environmentId,
        ReleaseId releaseId,
        DeploymentId deploymentId,
        DeploymentResult deploymentResult,
        DateTime deploymentDate,
        string? notes)
    {
        var release = GetRelease(environmentId, releaseId);
        release.InternalDeployments.EnsureDeploymentDoesntExist(deploymentId);

        // create the new deployment.
        var deployedEvt = new ReleaseDeployedEvent(
            deploymentId,
            deploymentDate,
            Id,
            environmentId,
            releaseId,
            deploymentResult,
            notes);
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
    
    internal void SetOutOfDate(EnvironmentId environmentId, ReleaseId releaseId, bool isOutOfDate)
    {
        if (isOutOfDate)
        {
            PlayEvent(new ReleaseValueBecameOld(Id, environmentId, releaseId));
            return;
        }
        
        PlayEvent(new ReleaseValueBecameCurrent(Id, environmentId, releaseId));
    }

    /// <summary>
    /// When deploying: see if any other release for the environment is already
    /// deployed. If so, remove it.
    /// </summary>
    /// <param name="environmentId"></param>
    /// <param name="releaseId"></param>
    /// <param name="deploymentId"></param>
    internal void SetActiveDeploymentToRemoved(
        EnvironmentId environmentId, 
        DeploymentId deploymentId)
    {
        // see if any deployment for any release in the environment is
        // currently deployed.
        // if so, set it to removed.
        var deployed = GetEnvironment(environmentId)
            .Releases
            .SelectMany(r => r.Deployments
                .Where(d => d.Id != deploymentId)
                .Where(d => d.Status == DeploymentStatus.Deployed)
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
using System.Text.Json;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class SectionAggregate : AggregateBase<SectionId>
{
    internal SectionAggregate(SectionId id, string initialEnvironmentType, string @namespace, string sectionName)
    {
        Guards.HasValue(id, nameof(id));
        Guards.HasValue(initialEnvironmentType, nameof(initialEnvironmentType));
        Guards.HasValue(@namespace, nameof(@namespace));
        PlayEvent(new SectionCreatedEvent(id, @namespace, sectionName, initialEnvironmentType));
    }

    internal SectionAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) PlayEvent(evt);
        InternalSourceEvents.Clear();
    }

    internal ISet<string> InternalEnvironmentTypes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    //internal List<SchemaId> InternalSchemas { get; } = new();
    internal List<EnvironmentEntity> InternalEnvironments { get; } = new();

    //public IEnumerable<SchemaId> Schemas => InternalSchemas.AsReadOnly();
    public IEnumerable<EnvironmentEntity> Environments => InternalEnvironments.AsReadOnly();
    public string SectionName { get; internal set; }
    public string Namespace { get; internal set; }

    /// <summary>
    ///     Gets the environment types that can host this section.
    /// </summary>
    public ISet<string> EnvironmentTypes => InternalEnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

    internal EnvironmentEntity AddEnvironment(EnvironmentId environmentId, string environmentType,
        string environmentName)
    {
        PlayEvent(new EnvironmentAddedToSectionEvent(environmentId, Id, environmentType, environmentName));
        return GetEnvironment(environmentName);
    }


    public EnvironmentEntity GetEnvironment(string name)
    {
        return InternalEnvironments.Single(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public EnvironmentEntity GetEnvironment(EnvironmentId environmentId)
    {
        return InternalEnvironments.GetEnvironment(environmentId);
    }

    internal void PlayEvent(IDomainEvent evt)
    {
        SectionAggregateEventHandlers.Play(this, evt);
        InternalSourceEvents.Add(evt);
    }

    public ReleaseEntity GetRelease(EnvironmentId environmentId, ReleaseId releaseId)
    {
        return GetEnvironment(environmentId).InternalReleases.GetRelease(releaseId);
    }

    public DeploymentEntity SetDeployed(DeploymentId deploymentId,
        ReleaseId releaseId,
        EnvironmentId environmentId,
        DeploymentResult deploymentResult,
        DateTime deploymentDate,
        string? notes = null)
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

    internal void SetReleaseValueChanged(EnvironmentId environmentId, ReleaseId releaseId, bool isOutOfDate)
    {
        var current = !isOutOfDate;
        var release = GetRelease(environmentId, releaseId);

        // changed from CURRENT to OUT OF DATE
        if (!release.IsOutOfDate && isOutOfDate)
        {
            PlayEvent(new ReleaseValueBecameOld(Id, environmentId, releaseId));
            return;
        }

        // changed from OUT OF DATE to CURRENT
        if (release.IsOutOfDate && current)
        {
            PlayEvent(new ReleaseValueBecameCurrent(Id, environmentId, releaseId));
        }

        // otherwise, nothing changed, so nothing to do
    }

    internal void PromoteTo(string environmentType)
    {
        PlayEvent(new SectionPromotedEvent(Id, environmentType));
    }

    /// <summary>
    ///     When deploying: see if any other release for the environment is already
    ///     deployed. If so, remove it.
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

    internal ReleaseEntity CreateRelease(ReleaseId releaseId,
        EnvironmentId environmentId,
        VariableSetId? variableSetId,
        SchemaId schemaId,
        JsonDocument value,
        JsonDocument resolved)
    {
        var evt = new ReleaseCreatedEvent(
            DateTime.UtcNow,
            releaseId,
            environmentId,
            Id,
            schemaId,
            variableSetId,
            value,
            resolved);
        PlayEvent(evt);
        return GetRelease(environmentId, releaseId);
    }
}
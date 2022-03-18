using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class SectionAggregate : AggregateBase<SectionId>
{
    private readonly ISet<string> _environmentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<EnvironmentEntity> _environments = new();

    public SectionAggregate()
    {
    }
    
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

    [JsonInclude]
    public string SectionName { get; private set; }
    
    [JsonInclude]
    public string Namespace { get; private set; }

    /// <summary>
    ///     Gets the environment types that can host this section.
    /// </summary>
    [JsonInclude]
    public ISet<string> EnvironmentTypes
    {
        get => _environmentTypes.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        internal init => _environmentTypes = value.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    [JsonInclude]
    public IEnumerable<EnvironmentEntity> Environments
    {
        get => _environments.AsReadOnly();
        internal init => _environments = value.ToList();
    }

    internal EnvironmentEntity AddEnvironment(EnvironmentId environmentId, string environmentType,
        string environmentName)
    {
        // validations are in the section service
        PlayEvent(new EnvironmentAddedToSectionEvent(environmentId, Id, environmentType, environmentName));
        return GetEnvironment(environmentName);
    }


    public EnvironmentEntity GetEnvironment(string name)
    {
        return _environments.Single(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public EnvironmentEntity GetEnvironment(EnvironmentId environmentId)
    {
        return _environments.GetEnvironment(environmentId);
    }

    public ReleaseEntity GetRelease(EnvironmentId environmentId, ReleaseId releaseId)
    {
        return GetEnvironment(environmentId)._releases.GetRelease(releaseId);
    }

    public DeploymentEntity SetDeployed(DeploymentId deploymentId,
        ReleaseId releaseId,
        EnvironmentId environmentId,
        DeploymentResult deploymentResult,
        DateTime deploymentDate,
        string? notes = null)
    {
        var release = GetRelease(environmentId, releaseId);
        release._deployments.EnsureDeploymentDoesntExist(deploymentId);

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
            ._deployments
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
    /// <param name="newDeploymentId">TODO: do we really need this? awkward.</param>
    protected internal void SetActiveDeploymentToRemoved(
        EnvironmentId environmentId,
        DeploymentId newDeploymentId)
    {
        // see if any deployment for any release in the environment is
        // currently deployed.
        // if so, set it to removed.
        var deployed = GetEnvironment(environmentId)
            .Releases
            .SelectMany(r => r.Deployments
                .Where(d => d.Id != newDeploymentId)
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
            $"Replaced by ReleaseId={deployed.Release.Id.Id}, Deployment Id={newDeploymentId.Id}");
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

    private void PlayEvent(IDomainEvent evt)
    {
        switch (evt)
        {
            case SectionCreatedEvent create:
                Id = create.SectionId;
                SectionName = create.SectionName;
                Namespace = create.Namespace;
                _environmentTypes.Add(create.InitialEnvironmentType);
                break;
            case EnvironmentAddedToSectionEvent environmentAdded:
                _environments.Add(new EnvironmentEntity(
                    environmentAdded.EnvironmentId,
                    environmentAdded.EnvironmentType,
                    environmentAdded.EnvironmentName));
                break;
            case ReleaseCreatedEvent releaseCreated:
            {
                var env = GetEnvironment(releaseCreated.EnvironmentId);
                var release = new ReleaseEntity(
                    releaseCreated.ReleaseId,
                    releaseCreated.SchemaId,
                    releaseCreated.ModelValue,
                    releaseCreated.ResolvedValue,
                    releaseCreated.VariableSetId,
                    releaseCreated.CreateDate);
                env._releases.Add(release);
                break;
            }
            case ReleaseDeployedEvent deployed:
            {
                // if an active deployment exists, remove it
                SetActiveDeploymentToRemoved(deployed.EnvironmentId, deployed.DeploymentId);

                var release = GetRelease(deployed.EnvironmentId, deployed.ReleaseId);
                release.SetDeployed(true);
                release._deployments.Add(new DeploymentEntity(
                    deployed.DeploymentId,
                    deployed.DeploymentDate,
                    deployed.DeploymentResult,
                    deployed.Notes));
                break;
            }
            case DeploymentRemovedEvent removed:
            {
                // todo: need a property for the date
                var release = GetRelease(removed.EnvironmentId, removed.ReleaseId);
                release.SetDeployed(false);
                release._deployments.GetDeployment(removed.DeploymentId)
                    .RemovedDeployment(removed.EventDate, removed.RemoveReason);
                break;
            }
            case ReleaseValueBecameOld outOfDate:
                GetRelease(outOfDate.EnvironmentId, outOfDate.ReleaseId).SetOutOfDate(true);
                break;
            case ReleaseValueBecameCurrent current:
                GetRelease(current.EnvironmentId, current.ReleaseId).SetOutOfDate(false);
                break;
            case SectionPromotedEvent promoted:
                _environmentTypes.Add(promoted.EnvironmentType);
                break;
            default:
                throw new NotImplementedException("Unhandled event: " + evt.GetType().FullName);
        }

        InternalSourceEvents.Add(evt);
    }
}
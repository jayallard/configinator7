using System.Text.Json;
using Allard.Json;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    // todo: make properties immutable
    internal List<DeploymentHistoryEntity> InternalDeployments { get; } = new();
    public IEnumerable<DeploymentHistoryEntity> Deployments => InternalDeployments.AsReadOnly();
    public EnvironmentEntity ParentEnvironment { get; }
    public JsonDocument ModelValue { get; }
    public JsonDocument ResolvedValue { get; }
    public TokenSetComposed? TokenSet { get; }
    public SchemaEntity Schema { get; }

    public ReleaseEntity(
        ReleaseId id,
        EnvironmentEntity parent,
        SchemaEntity schema,
        JsonDocument modelValue,
        JsonDocument resolvedValue,
        TokenSetComposed? tokenSet) : base(id)
    {
        ParentEnvironment = parent;
        Schema = schema;
        ModelValue = modelValue;
        ResolvedValue = resolvedValue;
        TokenSet = tokenSet;
    }

    public DeploymentHistoryEntity GetDeployment(DeploymentHistoryId deploymentHistoryId) =>
        InternalDeployments.GetDeployment(deploymentHistoryId);

    public DeploymentHistoryEntity SetDeployed(DeploymentHistoryId deploymentHistoryId, DateTime deploymentDate)
    {
        var section = ParentEnvironment.ParentSection;
        Deployments.EnsureDeploymentDoesntExist(deploymentHistoryId);

        // if an active deployment exists, remove it
        SetActiveDeploymentToRemoved(deploymentHistoryId, section);

        // create the new deployment.
        var deployedEvt = new ReleaseDeployedEvent(
            deploymentHistoryId,
            deploymentDate,
            section.Id,
            ParentEnvironment.Id,
            Id);
        section.PlayEvent(deployedEvt);
        return GetDeployment(deploymentHistoryId);
    }

    private void SetActiveDeploymentToRemoved(DeploymentHistoryId deploymentHistoryId, SectionEntity section)
    {
        var deployed = Deployments.SingleOrDefault(d => d.IsDeployed);
        if (deployed is null || deployed.Id == deploymentHistoryId) return;
        var removedEvent = new DeploymentRemovedEvent(
            deployed.Id,
            section.Id,
            ParentEnvironment.Id,
            Id,
            "Replaced by Deployment Id=" + Id.Id);
        section.PlayEvent(removedEvent);
    }
}
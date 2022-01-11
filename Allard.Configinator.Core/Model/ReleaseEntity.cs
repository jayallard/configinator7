using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    // todo: make properties immutable
    internal List<DeploymentHistoryEntity> InternalDeployments { get; } = new();
    public IEnumerable<DeploymentHistoryEntity> Deployments => InternalDeployments.AsReadOnly();
    public EnvironmentEntity ParentEnvironment { get; }
    public JObject ModelValue { get; }
    public JObject ResolvedValue { get; }
    public TokenSetComposed? TokenSet { get; }
    public SchemaEntity Schema { get; }

    public DeploymentHistoryEntity SetDeployed(DeploymentHistoryId deploymentHistoryId, DateTime deploymentDate)
    {
        var section = ParentEnvironment.ParentSection;

        if (Deployments.Any(d => d.Id.Equals(deploymentHistoryId)))
            throw new InvalidOperationException("Deployment already exists. Id=" + deploymentHistoryId.Id);

        // if an active deployment exists, remove it
        var deployed = Deployments.SingleOrDefault(d => d.IsDeployed);
        if (deployed != null && deployed.Id != deploymentHistoryId)
        {
            var removedEvent = new DeploymentRemovedSourceEvent(
                deployed.Id,
                section.Id,
                ParentEnvironment.Id,
                Id,
                "Replaced by Deployment Id=" + Id.Id);
            section.PlaySourceEvent(removedEvent);
        }

        // create the new deployment.
        var deployedEvt = new ReleaseDeployedSourceEvent(
            deploymentHistoryId,
            deploymentDate,
            section.Id,
            ParentEnvironment.Id,
            Id);
        section.PlaySourceEvent(deployedEvt);
        return GetDeployment(deploymentHistoryId);
    }

    public DeploymentHistoryEntity GetDeployment(DeploymentHistoryId deploymentHistoryId) =>
        InternalDeployments.Single(d => d.Id.Equals(deploymentHistoryId));

    public ReleaseEntity(
        ReleaseId id,
        EnvironmentEntity parent,
        SchemaEntity schema,
        JObject modelValue,
        JObject resolvedValue,
        TokenSetComposed? tokenSet) : base(id)
    {
        ParentEnvironment = parent;
        Schema = schema;
        ModelValue = modelValue;
        ResolvedValue = resolvedValue;
        TokenSet = tokenSet;
    }
}
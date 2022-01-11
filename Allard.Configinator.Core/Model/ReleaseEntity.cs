using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    // todo: make properties immutable
    internal List<DeploymentEntity> InternalDeployments { get; } = new();
    public IEnumerable<DeploymentEntity> Deployments => InternalDeployments.AsReadOnly();
    public EnvironmentEntity ParentEnvironment { get; }
    public JObject ModelValue { get; }
    public JObject ResolvedValue { get; }
    public TokenSetComposed? TokenSet { get; }
    public ConfigurationSchema Schema { get; }

    public DeploymentEntity SetDeployed(DeploymentId deploymentId, DateTime deploymentDate)
    {
        var section = ParentEnvironment.ParentSection;
        var environmentName = ParentEnvironment.EnvironmentName;

        if (Deployments.Any(d => d.Id.Equals(deploymentId)))
            throw new InvalidOperationException("Deployment already exists. Id=" + deploymentId.Id);

        // if an active deployment exists, remove it
        // var deployed = Deployments.SingleOrDefault(d => d.IsDeployed);
        // if (deployed != null)
        // {
        //     var removedEvent = new ReleaseRemovedSourceEvent(
        //         section.SectionName,
        //         environmentName,
        //         Id,
        //         "Replaced by Deployment Id=" + Id.Id);
        //     section.PlaySourceEvent(removedEvent);
        // }

        // create the new deployment.
        var deployedEvt = new ReleaseDeployedSourceEvent(
            deploymentId,
            deploymentDate,
            section.SectionName,
            environmentName,
            Id);
        section.PlaySourceEvent(deployedEvt);
        return GetDeployment(deploymentId);
    }

    public DeploymentEntity GetDeployment(DeploymentId deploymentId) =>
        InternalDeployments.Single(d => d.Id.Equals(deploymentId));

    public ReleaseEntity(
        ReleaseId id,
        EnvironmentEntity parent,
        ConfigurationSchema schema,
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
namespace Allard.Configinator.Core.Model;

public class DeploymentEntity : EntityBase<DeploymentId>
{
    public DateTime DeploymentDate { get; }
    
    public DateTime? RemovedDate { get; private set; }
    public string? RemoveReason { get; private set; }
    public bool IsDeployed { get; private set; } = true;

    internal void RemovedDeployment(DateTime removeDate, string removeReason)
    {
        if (!IsDeployed) throw new InvalidOperationException("not deployed");
        RemovedDate = removeDate;
        RemoveReason = removeReason;
        IsDeployed = false;
    }
    
    public DeploymentEntity(
        DeploymentId id,
        DateTime deploymentDate)
    {
        Id = id;
        DeploymentDate = deploymentDate;
    }
}
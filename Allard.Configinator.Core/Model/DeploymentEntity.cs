namespace Allard.Configinator.Core.Model;

public class DeploymentEntity : EntityBase<DeploymentId>
{
    public DateTime DeploymentDate { get; }
    public DateTime? RemovedDate { get; private set; }
    public string? RemoveReason { get; private set; }
    public bool IsDeployed { get; private set; } = true;
    
    public string? Notes { get; private set; }

    internal void RemovedDeployment(DateTime removeDate, string removeReason)
    {
        if (!IsDeployed) throw new InvalidOperationException("not deployed");
        RemovedDate = removeDate;
        RemoveReason = removeReason;
        IsDeployed = false;
    }
    
    internal DeploymentEntity(
        DeploymentId id,
        DateTime deploymentDate,
        string notes)
    {
        Id = id;
        DeploymentDate = deploymentDate;
        Notes = notes;
    }
}
using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

public class DeploymentHistoryEntity : EntityBase<DeploymentHistoryId>
{
    public ReleaseEntity ParentRelease { get; }
    public DateTime DeploymentDate { get; }
    public DeploymentStatus Status { get; private set; }
    public string? Reason { get; }
    public DeploymentHistoryEntity(
        DeploymentHistoryId historyId, 
        ReleaseEntity parentRelease,
        DateTime deploymentDate, 
        DeploymentStatus status, 
        string? reason) : base(historyId)
    {
        ParentRelease = parentRelease;
        DeploymentDate = deploymentDate;
        Status = status;
        Reason = reason;
    }

    public bool IsDeployed => Status == DeploymentStatus.Deployed;
    internal void SetRemoved() => Status  = DeploymentStatus.Removed;
}
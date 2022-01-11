using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

public class DeploymentHistoryEntity : EntityBase<DeploymentHistoryId>
{
    public ReleaseEntity ParentRelease { get; }
    public DateTime DeploymentDate { get; }
    public DeploymentAction Action { get; private set; }
    public string? Reason { get; }
    public DeploymentHistoryEntity(
        DeploymentHistoryId historyId, 
        ReleaseEntity parentRelease,
        DateTime deploymentDate, 
        DeploymentAction action, 
        string? reason) : base(historyId)
    {
        ParentRelease = parentRelease;
        DeploymentDate = deploymentDate;
        Action = action;
        Reason = reason;
    }

    public bool IsDeployed => Action == DeploymentAction.Deployed;
    internal void SetRemoved() => Action  = DeploymentAction.Removed;
}
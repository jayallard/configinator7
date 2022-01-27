namespace Allard.Configinator.Core.Model;

public class DeploymentHistoryEntity : EntityBase<DeploymentHistoryId>
{
    public DateTime DeploymentDate { get; }
    public DeploymentHistoryType HistoryType { get; private set; }
    public string? Reason { get; }
    public bool IsDeployed { get; private set; }
    internal void SetRemoved() => IsDeployed = false;
    
    public DeploymentHistoryEntity(
        DeploymentHistoryId historyId,
        DateTime deploymentDate,
        DeploymentHistoryType historyType,
        string? reason) : base(historyId)
    {
        DeploymentDate = deploymentDate;
        HistoryType = historyType;
        Reason = reason;
        IsDeployed = historyType == DeploymentHistoryType.Deployed;
    }
}
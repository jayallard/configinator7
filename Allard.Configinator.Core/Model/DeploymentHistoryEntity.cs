using System.Data.SqlTypes;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class DeploymentHistoryEntity : EntityBase<DeploymentHistoryId>
{
    public ReleaseEntity ParentRelease { get; }
    public DateTime DeploymentDate { get; }
    public DeploymentHistoryType HistoryType { get; private set; }
    public string? Reason { get; }
    public bool IsDeployed { get; private set; }
    internal void SetRemoved() => IsDeployed = false;
    
    public DeploymentHistoryEntity(
        DeploymentHistoryId historyId,
        ReleaseEntity parentRelease,
        DateTime deploymentDate,
        DeploymentHistoryType historyType,
        string? reason) : base(historyId)
    {
        ParentRelease = parentRelease;
        DeploymentDate = deploymentDate;
        HistoryType = historyType;
        Reason = reason;
        IsDeployed = historyType == DeploymentHistoryType.Deployed;
    }
}
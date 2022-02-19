namespace Allard.Configinator.Core.Model;

public class DeploymentEntity : EntityBase<DeploymentId>
{
    internal DeploymentEntity(
        DeploymentId id,
        DateTime deploymentDate,
        DeploymentResult deploymentResult,
        string? notes)
    {
        Id = id;
        DeploymentDate = deploymentDate;
        DeploymentResult = deploymentResult;
        Status = deploymentResult.IsSuccess ? DeploymentStatus.Deployed : DeploymentStatus.Error;
        Notes = notes;
    }

    public DateTime DeploymentDate { get; }
    public DateTime? RemovedDate { get; private set; }
    public string? RemoveReason { get; private set; }

    public DeploymentStatus Status { get; private set; }
    public DeploymentResult? DeploymentResult { get; }
    public string? Notes { get; }

    internal void RemovedDeployment(DateTime removeDate, string removeReason)
    {
        if (Status != DeploymentStatus.Deployed) throw new InvalidOperationException("not deployed");
        RemovedDate = removeDate;
        RemoveReason = removeReason;
        Status = DeploymentStatus.Removed;
    }
}

public enum DeploymentStatus
{
    NotDeployed,
    Deployed,
    Removed,
    Unknown,
    Error
}
using System.Text.Json.Serialization;

namespace Allard.Configinator.Core.Model;

public class DeploymentEntity : EntityBase<DeploymentId>
{
    public DeploymentEntity(
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

    [JsonInclude]
    public DateTime DeploymentDate { get; private init; }
    
    [JsonInclude]
    public DateTime? RemovedDate { get; private set; }
    [JsonInclude]
    public string? RemoveReason { get; private set; }
    
    [JsonInclude]
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
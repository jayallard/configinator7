﻿using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

public class DeploymentEntity : EntityBase<DeploymentId>
{
    public ReleaseEntity ParentRelease { get; }
    public DateTime DeploymentDate { get; }
    public DeploymentAction Action { get; private set; }
    public string? Reason { get; }
    public DeploymentEntity(
        DeploymentId id, 
        ReleaseEntity parentRelease,
        DateTime deploymentDate, 
        DeploymentAction action, 
        string? reason) : base(id)
    {
        ParentRelease = parentRelease;
        DeploymentDate = deploymentDate;
        Action = action;
        Reason = reason;
    }

    public bool IsDeployed => Action == DeploymentAction.Deployed;
    internal void SetDeployed() => Action = DeploymentAction.Deployed;
    internal void SetRemoved() => Action  = DeploymentAction.Removed;
}
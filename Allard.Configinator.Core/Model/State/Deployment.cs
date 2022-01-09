namespace Allard.Configinator.Core.Model.State;

public record Deployment(DateTime DeploymentDate, DeploymentAction Action, string Reason)
{
    public bool IsDeployed { get; set; }
}
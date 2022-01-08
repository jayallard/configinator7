namespace Allard.Configinator.Core.Model;

public record Deployment(DateTime DeploymentDate, DeploymentAction Action, string Reason)
{
    public bool IsDeployed { get; set; }
}
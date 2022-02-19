namespace Allard.Configinator.Deployer.Abstractions;

public interface IDeployer
{
    /// <summary>
    ///     Returns configuration information that Configinator can display on the DEPLoy page.
    /// </summary>
    /// <returns></returns>
    Task<DeployerDescription> GetDescriptionAsync(DeployRequest Request, CancellationToken cancellationToken = default);

    Task<DeployResult> DeployAsync(DeployRequest Request, CancellationToken cancellationToken = default);
}
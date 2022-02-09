namespace Allard.Configinator.Deployer.Abstractions;

public interface IDeployerFactory
{
    Task<IDeployer> GetDeployer(DeployRequest request);
}
using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public interface IDeploymentConfigurationFactory
{
    Task<MemoryDeploymentConfiguration> GetConfigurationAsync(DeployRequest request);
}
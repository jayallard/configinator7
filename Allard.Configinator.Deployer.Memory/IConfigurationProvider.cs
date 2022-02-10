using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public interface IConfigurationProvider
{
    Task<MemoryDeploymentConfiguration> GetConfigurationAsync(DeployRequest request);
}
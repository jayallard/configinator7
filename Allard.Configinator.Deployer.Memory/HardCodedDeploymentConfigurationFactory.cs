using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public class HardCodedDeploymentConfigurationFactory : IDeploymentConfigurationFactory
{
    public Task<MemoryDeploymentConfiguration> GetConfigurationAsync(DeployRequest request)
    {
        if (request.Environment.EnvironmentName.StartsWith("dev"))
        {
            return Task.FromResult(new MemoryDeploymentConfiguration
            {
                RegionName = "jay-east-1",
                UserName = "e1-username",
                Password = "e1-password"
            });
        }

        if (request.Environment.EnvironmentName.StartsWith("production"))
        {
            return Task.FromResult(new MemoryDeploymentConfiguration
            {
                RegionName = "jay-west-4",
                UserName = "w4-username",
                Password = "24-password"
            });
        }

        throw new InvalidOperationException("Configuration undefined");
    }
}
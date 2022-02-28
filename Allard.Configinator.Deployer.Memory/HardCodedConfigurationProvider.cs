using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public class HardCodedConfigurationProvider : IConfigurationProvider
{
    public Task<MemoryDeploymentConfiguration> GetConfigurationAsync(DeployRequest request)
    {
        if (request.DeploymentEnvironment.EnvironmentName.StartsWith("dev"))
            return Task.FromResult(new MemoryDeploymentConfiguration
            {
                Path = "/a/b/c/" + request.DeploymentEnvironment.EnvironmentName,
                RegionName = "jay-east-1",
                UserName = "e1-username",
                Password = "e1-password"
            });
        
        if (request.DeploymentEnvironment.EnvironmentName.StartsWith("staging"))
            return Task.FromResult(new MemoryDeploymentConfiguration
            {
                Path = "/x/y/z/" + request.DeploymentEnvironment.EnvironmentName,
                RegionName = "jay-west-4",
                UserName = "w4-username",
                Password = "24-password"
            });

        if (request.DeploymentEnvironment.EnvironmentName.StartsWith("production"))
            return Task.FromResult(new MemoryDeploymentConfiguration
            {
                Path = "/x/y/z/" + request.DeploymentEnvironment.EnvironmentName,
                RegionName = "jay-west-2",
                UserName = "w4-username",
                Password = "24-password"
            });

        throw new InvalidOperationException("Configuration undefined");
    }
}
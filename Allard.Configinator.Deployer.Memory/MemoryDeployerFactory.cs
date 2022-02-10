using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public class MemoryDeployerFactory : IDeployerFactory
{
    private readonly IConfigurationProvider _configurationProvider;

    public MemoryDeployerFactory(IConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }

    public Task<IDeployer> GetDeployer(DeployRequest request) =>
        Task.FromResult((IDeployer)new MemoryDeployer(_configurationProvider));
}
using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

/// <summary>
/// A single application may have many deployers.
/// But, in this case, we only have one. So, inject the one.
/// TODO: this can be moved to abstractions, or similar.
/// This isn't memory specific.
/// </summary>
public class SingleDeployerFactory : IDeployerFactory
{
    private readonly IDeployer _deployer;

    public SingleDeployerFactory(IDeployer deployer)
    {
        _deployer = deployer;
    }

    public Task<IDeployer> GetDeployer(DeployRequest request) =>
        Task.FromResult(_deployer);
}
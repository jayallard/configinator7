using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

public record DeploymentId : IdBase<Deployment>
{
    public DeploymentId(long id) : base(id)
    {
    }
}
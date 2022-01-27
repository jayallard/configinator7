using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record DeploymentId : IdBase
{
    public DeploymentId(long id) : base(id)
    {
    }
}
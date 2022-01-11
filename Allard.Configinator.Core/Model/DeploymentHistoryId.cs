namespace Allard.Configinator.Core.Model;

public record DeploymentHistoryId : IdBase<DeploymentHistoryEntity>
{
    public DeploymentHistoryId(long id) : base(id)
    {
    }
}
namespace Allard.Configinator.Core.Model;

public class EnvironmentId : IdBase<EnvironmentEntity, EnvironmentId>
{
    public EnvironmentId(long id) : base(id)
    {
    }

    public ReleaseEntity? GetRelease(ReleaseId id) => null;
}
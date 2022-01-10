namespace Allard.Configinator.Core.Model;

public record EnvironmentId : IdBase<EnvironmentEntity>
{
    public EnvironmentId(long id) : base(id)
    {
    }
    public ReleaseEntity? GetRelease(ReleaseId id) => null;
}
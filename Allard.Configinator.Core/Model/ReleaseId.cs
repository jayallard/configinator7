namespace Allard.Configinator.Core.Model;

public record ReleaseId : IdBase<ReleaseEntity>
{
    public ReleaseId(long id) : base(id)
    {
    }
}
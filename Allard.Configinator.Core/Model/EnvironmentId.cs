using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record EnvironmentId : IdBase
{
    public EnvironmentId(long id) : base(id)
    {
    }

    public ReleaseEntity? GetRelease(ReleaseId id)
    {
        return null;
    }
}
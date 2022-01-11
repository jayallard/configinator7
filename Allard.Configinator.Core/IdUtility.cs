using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public static class IdUtility
{
    public static SectionId SectionId(long id) => new(id);
    public static EnvironmentId EnvironmentId(long id) => new(id);
    public static ReleaseId ReleaseId(long id) => new(id);
    public static DeploymentId DeploymentId(long id) => new(id);
}
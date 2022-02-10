using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public static class IdUtility
{
    public static SectionId NewSectionId(long id) => new(id);
    public static EnvironmentId NewEnvironmentId(long id) => new(id);
    public static ReleaseId NewReleaseId(long id) => new(id);
    public static DeploymentId NewDeploymentId(long id) => new(id);
    public static SectionSchemaId NewSchemaId(long id) => new(id);
    public static VariableSetId NewVariableSetId(long id) => new(id);
}
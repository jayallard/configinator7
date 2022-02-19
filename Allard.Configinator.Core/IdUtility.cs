using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public static class IdUtility
{
    public static SectionId NewSectionId(long id)
    {
        return new(id);
    }

    public static EnvironmentId NewEnvironmentId(long id)
    {
        return new(id);
    }

    public static ReleaseId NewReleaseId(long id)
    {
        return new(id);
    }

    public static DeploymentId NewDeploymentId(long id)
    {
        return new(id);
    }

    public static SchemaId NewSchemaId(long id)
    {
        return new(id);
    }

    public static VariableSetId NewVariableSetId(long id)
    {
        return new(id);
    }
}
using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public static class IdUtility
{
    public static SectionId NewSectionId(long id)
    {
        return new SectionId(id);
    }

    public static EnvironmentId NewEnvironmentId(long id)
    {
        return new EnvironmentId(id);
    }

    public static ReleaseId NewReleaseId(long id)
    {
        return new ReleaseId(id);
    }

    public static DeploymentId NewDeploymentId(long id)
    {
        return new DeploymentId(id);
    }

    public static SchemaId NewSchemaId(long id)
    {
        return new SchemaId(id);
    }

    public static VariableSetId NewVariableSetId(long id)
    {
        return new VariableSetId(id);
    }
}
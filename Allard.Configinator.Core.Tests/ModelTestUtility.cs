using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Tests;

public static class ModelTestUtility
{
    public static readonly SemanticVersion Schema1Version = new(1, 0, 0);

    public static SectionAggregate CreateTestSection()
    {
        var section = new SectionAggregate(NewSectionId(0), "development", "ns", "s");
        section.AddEnvironment(NewEnvironmentId(0), "environmentType", "test1");
        return section;
    }
}
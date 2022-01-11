using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;
using NJsonSchema;
using NuGet.Versioning;
using static Allard.Configinator.Core.IdUtility;

namespace Allard.Configinator.Core.Tests;

public static class ModelTestUtility
{
    public static readonly SemanticVersion Schema1Id = new(1, 0, 0);

    public static SectionEntity CreateTestSection()
    {
        {
            var schema = new ConfigurationSchema(Schema1Id, JsonSchema.CreateAnySchema());
            var section = new SectionEntity(SectionId(0), "s", "p", schema);
            section.AddEnvironment(EnvironmentId(0), "test1");
            return section;
        }
    }
}
using System.Text.Json;
using Allard.Configinator.Core.Model;
using NJsonSchema;
using NuGet.Versioning;
using static Allard.Configinator.Core.IdUtility;

namespace Allard.Configinator.Core.Tests;

public static class ModelTestUtility
{
    public static readonly SectionSchemaId Schema1Id = NewSchemaId(0);
    public static readonly SemanticVersion Schema1Version = new(1, 0, 0);

    public static SectionAggregate CreateTestSection()
    {
        {
            var schema = new SectionSchemaEntity(Schema1Id, Schema1Version, JsonDocument.Parse("{}"));
            var section = new SectionAggregate(NewSectionId(0), "s", "p", schema);
            section.AddEnvironment(NewEnvironmentId(0), "test1");
            return section;
        }
    }
}
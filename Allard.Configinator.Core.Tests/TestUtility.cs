using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Tests;

public static class TestUtility
{
    public static readonly SemanticVersion Schema1Version = new(1, 0, 0);

    public static SectionAggregate CreateTestSection()
    {
        var section = new SectionAggregate(NewSectionId(0), "development", "/ns", "s");
        section.AddEnvironment(NewEnvironmentId(0), "environmentType", "test1");
        return section;
    }
    
    public static JsonDocument EmptyDoc()
    {
        return JsonDocument.Parse("{}");
    }

    public static JsonDocument TestSchema()
    {
        //lang=json
        return JsonDocument.Parse(
            "{\n  \"properties\": {\n    \"something\": {\n      \"type\": \"string\"\n    }\n  }\n}");
    }

    public static JsonDocument TestValue()
    {
        return JsonDocument.Parse("{ \"something\": \"value\" }");
    }

    public static DeploymentResult EmptyDeploymentResult()
    {
        return new(true, new List<DeploymentResultMessage>().AsReadOnly());
    }
}
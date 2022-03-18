using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Xunit.Abstractions;

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

    public static void EnsureSerializesAndDeserializesToSameThing<T>(T obj, ITestOutputHelper _testOutputHelper)
    {
        var serialized = ModelJsonUtility.Serialize(obj);
        var deserialized = ModelJsonUtility.Deserialize<T>(serialized);
        
        EnsureObjectsHaveSameJson(obj, deserialized, _testOutputHelper);
    }
    
    private static void EnsureObjectsHaveSameJson(object expected, object actual, ITestOutputHelper _testOutputHelper)
    {
        var eText = ModelJsonUtility.Serialize(expected);
        var aText = ModelJsonUtility.Serialize(actual);
        var eJson = JToken.Parse(eText);
        var aJson = JToken.Parse(aText);
        
        _testOutputHelper.WriteLine(eText);
        _testOutputHelper.WriteLine("--------------------------------------");
        _testOutputHelper.WriteLine(aText);


        JToken.DeepEquals(eJson, aJson).Should().BeTrue();
    }
}
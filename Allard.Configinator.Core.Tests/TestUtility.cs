using System.Collections.Generic;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Xunit;
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

    /// <summary>
    /// Serialize expected and actual to json.
    /// Then deserialize to JSON objects.
    /// Make sure the JSON object are identical.
    /// This assures that serialization and deserialization don't work.
    /// This doesn't assure that every property is serialized; it doesn't know
    /// what should or shouldn't be. It just assures that the things that
    /// are serialized are propertly deserialized.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    /// <param name="_testOutputHelper"></param>
    private static void EnsureObjectsHaveSameJson(object expected, object actual, ITestOutputHelper _testOutputHelper)
    {
        var eText = ModelJsonUtility.Serialize(expected);
        var aText = ModelJsonUtility.Serialize(actual);
        var eJson = JToken.Parse(eText);
        var aJson = JToken.Parse(aText);

        var isMatch = JToken.DeepEquals(eJson, aJson);
        //if (isMatch) return;
        _testOutputHelper.WriteLine(eText);
        _testOutputHelper.WriteLine("--------------------------------------");
        _testOutputHelper.WriteLine(aText);
        isMatch.Should().BeTrue("The json docs don't match. See the OUTPUT for details.");
    }


    /// <summary>
    /// Wraps an object in an object[], so it can be passed to a test
    /// Theory with method parameters.
    /// Additionally, it wraps the object so that the display name
    /// can be set either explicitly, or default to type name
    /// of the value.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object[] AsTestData(this object obj, string? displayName = null) => new object[] {new TestParameterWrapper(obj, displayName)};
}

public class TestParameterWrapper
{
    public TestParameterWrapper(object value, string? displayName = null)
    {
        this.Value = value;
        DisplayName = displayName ?? value.GetType().Name;
    }

    public object Value { get; }
    
    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
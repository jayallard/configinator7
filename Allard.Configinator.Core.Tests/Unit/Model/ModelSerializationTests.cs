using System;
using System.Collections.Generic;
using System.Linq;
using Allard.Configinator.Core.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class ModelSerializationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ModelSerializationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [MemberData(nameof(ObjectsToSerialize))]
    public void SerializeAndDeserialize(DisplayNameWrapper obj)
    {
        EnsureSerializesAndDeserializesToSameThing(obj.Data, _testOutputHelper);
    }

    [Fact]
    public void JsonValueTests()
    {
        var evt = new VariableValueSetEvent("n", "n", new JArray());
        var serialized = ModelJsonUtility.Serialize(evt);
        var deserialized = ModelJsonUtility.Deserialize<VariableValueSetEvent>(serialized);
        // _testOutputHelper.WriteLine(deserialized.TokenValue.ToString());
    }


    private static IEnumerable<object> ObjectsToSerialize()
    {
        // the ids all work the same. they're equivalent.
        yield return new SectionId(345).Displayable();
        yield return new ReleaseId(345).Displayable();
        yield return new SchemaId(345).Displayable();

        // custom type converter needed
        yield return SemanticVersion.Parse("1.0.0-prerelease+3333").Displayable();

        // aggregates are complicated. there's custom
        // conversion code. these really need testing. be thorough; fully
        // populate them.
        yield return new NamespaceAggregate(new NamespaceId(33), "/ns/a/b/c")
        {
            Schemas = new[] {new SchemaId(3), new SchemaId(4)}.ToHashSet(),
            Sections = new[] {new SectionId(99), new SectionId(100)}.ToHashSet(),
            VariableSets = new[] {new VariableSetId(22), new VariableSetId(23)}.ToHashSet()
        }.Displayable();

        yield return new SchemaAggregate(
                new SchemaId(3),
                new SectionId(5),
                "Development",
                "/na/a/b/c",
                new SchemaName("boo/1.0.0"),
                "description",
                TestSchema())
            .Promote("staging")
            .Promote("production")
            .Displayable();

        yield return CreateSectionAggregate().Displayable();

        // these objects are easily serializable.
        // will add them as needed, but not going to make an early effort
        // to add them all.
        // they are equivalent
        yield return new SectionCreatedEvent(new SectionId(1), "/ns", "section name", "development").Displayable();

        yield return new VariableValueSetEvent("boo", "a", new JArray()).Displayable();
    }

    private static SectionAggregate CreateSectionAggregate()
    {
        var section = new SectionAggregate(new SectionId(13), "development", "/ns/a/b", "section1");
        section.PromoteTo("staging");
        section.AddEnvironment(new EnvironmentId(22), "staging", "stg");
        section.AddEnvironment(new EnvironmentId(222), "production", "prod");
        section.CreateRelease(
            new ReleaseId(999),
            new EnvironmentId(22),
            new VariableSetId(27), new SchemaId(99),
            TestValue(),
            TestValue());

        var result = new DeploymentResult(true,
            new List<DeploymentResultMessage>
                    {new("source", "key", LogLevel.Critical, "boom", "exception")}
                .AsReadOnly());
        section.SetDeployed(
            new DeploymentId(1),
            new ReleaseId(999),
            new EnvironmentId(22),
            result,
            DateTime.Now,
            "notes");
        section.SetActiveDeploymentToRemoved(new EnvironmentId(22), new DeploymentId(22));
        return section;
    }
}
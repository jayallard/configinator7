using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Allard.Json;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Allard.Configinator.Core.Tests.Unit;

public class SchemaAggregateSerializationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SchemaAggregateSerializationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void SerializeAndDeserializeSchemaAggregate()
    {
        // arrange
        var schema = new SchemaAggregate(
            new SchemaId(3),
            new SectionId(5),
            "Development",
            "/na/a/b/c",
            new SchemaName("boo/1.0.0"),
            "description",
            TestSchema());
        schema.Promote("staging");
        schema.Promote("production");

        // act
        var serialized = ModelJsonUtility.Serialize(schema);
        var deserialized = ModelJsonUtility.Deserialize<SchemaAggregate>(serialized);

        // assert
        deserialized!.Description.Should().Be(schema.Description);
        deserialized.Namespace.Should().Be(schema.Namespace);
        deserialized.EnvironmentTypes.Should().BeEquivalentTo(schema.EnvironmentTypes);
        deserialized.SchemaName.Should().Be(schema.SchemaName);
        deserialized.SectionId.Should().Be(schema.SectionId);

        var expectedSchema = TestSchema().ToJsonNetJson();
        var actualSchema = deserialized.Schema.ToJsonNetJson();
        JToken.DeepEquals(expectedSchema, actualSchema).Should().BeTrue();

        // var serialized2 = JsonSerializer.Serialize(deserialized, Options);
        // _testOutputHelper.WriteLine(serialized);
        // _testOutputHelper.WriteLine("---------------------------------");
        // _testOutputHelper.WriteLine(serialized2);
    }

    [Fact]
    public async Task SemanticVersionTest()
    {
        var version = SemanticVersion.Parse("1.0.0-prerelease+3333");
        var serialized = ModelJsonUtility.Serialize(version);
        _testOutputHelper.WriteLine(serialized);
        var deserialized = ModelJsonUtility.Deserialize<SemanticVersion>(serialized);
        deserialized.Should().Be(version);
    }
}

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

    public static readonly JsonSerializerOptions ModelSerializerOptions = new()
    {
        Converters =
        {
            new SemanticVersionSerializer(),
            new IdConverter()
        },
        WriteIndented = true
    };

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
        var serialized = JsonSerializer.Serialize(schema, ModelSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<SchemaAggregate>(serialized, ModelSerializerOptions);

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
        var serialized = JsonSerializer.Serialize(version, ModelSerializerOptions);
        _testOutputHelper.WriteLine(serialized);
        var deserialized = JsonSerializer.Deserialize<SemanticVersion>(serialized, ModelSerializerOptions);
        deserialized.Should().Be(version);
    }
}

public class SemanticVersionSerializer : JsonConverter<SemanticVersion>
{
    public override SemanticVersion?
        Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        SemanticVersion.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToFullString());
}

public class EntityIdConverter : JsonConverter<IIdentity>
{
    public override IIdentity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt64();
        return (IIdentity) Activator.CreateInstance(typeToConvert, value);
    }

    public override void Write(Utf8JsonWriter writer, IIdentity value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Id);
    }
}

public class IdConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(IIdentity));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsAssignableTo(typeof(IIdentity)))
        {
            return new EntityIdConverter();
        }

        return null;
    }
}

public class SchemaNameConvert : JsonConverter<SchemaName>
{
    public override SchemaName? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value == null ? null : SchemaName.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, SchemaName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.FullName);
    }
}
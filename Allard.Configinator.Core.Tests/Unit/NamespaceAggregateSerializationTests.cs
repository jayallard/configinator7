using System.Linq;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit;

public class NamespaceAggregateSerializationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NamespaceAggregateSerializationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SerializeAndDeserializeNamespaceAggregate()
    {
        // arrange
        var ns = new NamespaceAggregate(new NamespaceId(33), "/ns/a/b/c")
        {
            Schemas = new[] {new SchemaId(3), new SchemaId(4)}.ToHashSet(),
            Sections = new[] {new SectionId(99), new SectionId(100)}.ToHashSet(),
            VariableSets = new[] {new VariableSetId(22), new VariableSetId(23)}.ToHashSet()
        };

        // act
        var serialized = ModelJsonUtility.Serialize(ns);
        _testOutputHelper.WriteLine(serialized);
        var deserialized = ModelJsonUtility.Deserialize<NamespaceAggregate>(serialized);
        
        // assert
        deserialized.Id.Should().Be(ns.Id);
        deserialized.Namespace.Should().Be(ns.Namespace);
        deserialized.Schemas.Should().BeEquivalentTo(ns.Schemas);
        deserialized.Sections.Should().BeEquivalentTo(ns.Sections);
        deserialized.VariableSets.Should().BeEquivalentTo(ns.VariableSets);
        deserialized.EntityId.Should().Be(deserialized.EntityId);
    }
}
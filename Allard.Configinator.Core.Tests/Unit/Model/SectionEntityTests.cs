using System.Linq;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;
using FluentAssertions;
using NJsonSchema;
using NuGet.Versioning;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class SectionEntityTests
{
    [Fact]
    public void CreateSection()
    {
        // arrange, act
        var section = new SectionEntity(new SectionId(0), "name", "path");
        
        // assert
        section.Name.Should().Be("name");
        section.Path.Should().Be("path");
    }

    [Fact]
    public void AddSchema()
    {
        // arrange
        var section = new SectionEntity(new SectionId(0), "name", "path");
        var schema = new ConfigurationSchema(new SemanticVersion(1, 0, 0), JsonSchema.CreateAnySchema());
        
        // act
        section.AddSchema(schema);
        
        // assert
        section.Schemas.Single().Should().Be(schema);
    }

    [Fact]
    public void AddEnvironment()
    {
        // arrange
        var section = new SectionEntity(new SectionId(0), "name", "path");
        
        // act
        section.AddEnvironment(new EnvironmentId(25), "dev");
        
        // assert
        section.Environments.Single().Id.Id.Should().Be(25);
        section.Environments.Single().Name.Should().Be("dev");
    }
}
using System;
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
        var schema = new ConfigurationSchema(new SemanticVersion(1, 0, 0), JsonSchema.CreateAnySchema());
        var section = new SectionEntity(new SectionId(0), "name", "path", schema);

        // assert
        section.SectionName.Should().Be("name");
        section.Path.Should().Be("path");
        section.Schemas.Single().Version.Should().Be(new SemanticVersion(1, 0, 0));
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
    public void AddSchemaThrowsExceptionIfAlreadyExists()
    {
        // arrange
        var section = new SectionEntity(new SectionId(0), "name", "path");
        var schema1 = new ConfigurationSchema(new SemanticVersion(1, 0, 0), JsonSchema.CreateAnySchema());
        var schema2 = new ConfigurationSchema(new SemanticVersion(1, 0, 0), JsonSchema.CreateAnySchema());

        // act
        section.AddSchema(schema1);
        var test = () => section.AddSchema(schema2);

        // assert
        test
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Schema already exists. Version=1.0.0");
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
        section.Environments.Single().EnvironmentName.Should().Be("dev");
    }

    [Fact]
    public void AddEnvironmentThrowsExceptionIfNameAlreadyExists()
    {
        // arrange
        var section = new SectionEntity(new SectionId(0), "name", "path");

        // act
        section.AddEnvironment(new EnvironmentId(25), "dev");
        var test = () => section.AddEnvironment(new EnvironmentId(26), "dev");

        // assert
        test
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Environment already exists. Name=dev");
    }

    [Fact]
    public void AddEnvironmentThrowsExceptionIfIdAlreadyExists()
    {
        // arrange
        var section = new SectionEntity(new SectionId(0), "name", "path");

        // act
        section.AddEnvironment(new EnvironmentId(25), "dev");
        var test = () => section.AddEnvironment(new EnvironmentId(25), "dev2");

        // assert
        test
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Environment already exists. Id=25");
    }
}
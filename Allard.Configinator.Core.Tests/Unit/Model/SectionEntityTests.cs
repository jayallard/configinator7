using System;
using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using NuGet.Versioning;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class SectionEntityTests
{
    [Fact]
    public void CreateSection()
    {
        // arrange, act
        var section = new SectionAggregate(new SectionId(0), "development","name", "path");

        // assert
        section.SectionName.Should().Be("name");
        section.OrganizationPath.Should().Be("path");
        section.Schemas.Single().Version.Should().Be(new SemanticVersion(1, 0, 0));
    }

    [Fact]
    public void AddSchema()
    {
        // arrange
        var section = new SectionAggregate(new SectionId(0), "Development","name", "path");

        // act
        section.AddSchema(new SectionSchemaId(0), new SemanticVersion(1, 0, 0), JsonDocument.Parse("{}"), "development");

        // assert
        section.Schemas.Count().Should().Be(1);
    }

    [Fact]
    public void AddSchemaThrowsExceptionIfVersionAlreadyExists()
    {
        // arrange
        var section = new SectionAggregate(NewSectionId(0), "Development", "name", "path");
        var schema1 = new SectionSchemaEntity(new SectionSchemaId(0), new SemanticVersion(1, 0, 0),
            JsonDocument.Parse("{}"), "development");
        var schema2 = new SectionSchemaEntity(new SectionSchemaId(1), new SemanticVersion(1, 0, 0),
            JsonDocument.Parse("{}"), "development");

        // act
        section.AddSchema(new SectionSchemaId(0), new SemanticVersion(1, 0, 0), JsonDocument.Parse("{}"), "development");
        var test = () =>
            section.AddSchema(new SectionSchemaId(1), new SemanticVersion(1, 0, 0), JsonDocument.Parse("{}"), "development");

        // assert
        test
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Schema already exists. Version=1.0.0");
    }

    [Fact]
    public void AddSchemaThrowsExceptionIdAlreadyExists()
    {
        // arrange
        var section = new SectionAggregate(NewSectionId(0), "Development", "name", "path");

        // act
        section.AddSchema(new SectionSchemaId(0), new SemanticVersion(1, 0, 0), JsonDocument.Parse("{}"), "development");
        var test = () =>
            section.AddSchema(new SectionSchemaId(0), new SemanticVersion(1, 1, 0), JsonDocument.Parse("{}"), "development");

        // assert
        test
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Schema already exists. Id=0");
    }

    [Fact]
    public void AddEnvironment()
    {
        // arrange
        var section = new SectionAggregate(new SectionId(0), "Development", "name", "path");

        // act
        section.InternalEnvironments.Add(new EnvironmentEntity(new EnvironmentId(25), "development", "dev"));

        // assert
        section.Environments.Single().Id.Id.Should().Be(25);
        section.Environments.Single().EnvironmentName.Should().Be("dev");
    }

    [Fact]
    public void AddEnvironmentThrowsExceptionIfNameAlreadyExists()
    {
        // arrange
        var section = new SectionAggregate(new SectionId(0), "Development", "name", "path");

        // act
        var test = () =>
            section.InternalEnvironments.Add(new EnvironmentEntity(new EnvironmentId(25), "development", "dev"));

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
        var section = new SectionAggregate(new SectionId(0), "Development", "name", "path");

        // act
        section.InternalEnvironments.Add(new EnvironmentEntity(new EnvironmentId(25), "development", "dev"));
        var test = () =>
            section.InternalEnvironments.Add(new EnvironmentEntity(new EnvironmentId(25), "development", "dev2"));

        // assert
        test
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Environment already exists. Id=25");
    }
}
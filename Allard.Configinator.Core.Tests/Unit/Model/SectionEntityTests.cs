using System;
using System.Linq;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class SectionEntityTests
{
    [Fact]
    public void CreateSection()
    {
        // arrange, act
        var section = new SectionAggregate(new SectionId(0), "development", "name");

        // assert
        section.SectionName.Should().Be("name");
    }


    [Fact]
    public void AddEnvironment()
    {
        // arrange
        var section = new SectionAggregate(new SectionId(0), "Development", "name");

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
        var section = new SectionAggregate(new SectionId(0), "Development", "name");

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
        var section = new SectionAggregate(new SectionId(0), "Development", "name");

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
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
        var section = new SectionAggregate(new SectionId(0), "development", "/ns", "name");

        // assert
        section.SectionName.Should().Be("name");
    }


    [Fact]
    public void AddEnvironment()
    {
        // arrange
        var section = new SectionAggregate(new SectionId(0), "Development", "/ns", "name");

        // act
        section.AddEnvironment(new EnvironmentId(25), "development", "dev");

        // assert
        section.Environments.Single().Id.Id.Should().Be(25);
        section.Environments.Single().EnvironmentName.Should().Be("dev");
    }
}
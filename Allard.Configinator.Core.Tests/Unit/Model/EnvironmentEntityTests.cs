using System.Linq;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class EnvironmentEntityTests
{
    [Fact]
    public void AddRelease()
    {
        // arrange
        var section = CreateTestSection();
        var environment1 = section.GetEnvironment("test1");

        // act
        var release = section.CreateRelease(
            NewReleaseId(0),
            environment1.Id,
            new VariableSetId(0), new SchemaId(0),
            JsonDocument.Parse("{}"),
            JsonDocument.Parse("{}"));

        // assert
        environment1.Releases.Single().Should().Be(release);
    }
}